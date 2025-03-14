//
//  GoogleFetchController.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 2/9/25.
//

import Foundation
import CoreData
import ReadiumShared

struct Book: Codable {
    var title: String = ""
    var authors: String = ""
    var publishedDate: String = ""
    var pageCount: Int64 = 0
    var bookSynopsis: String = ""
    var isbn13: String = ""
    var genre: String? = ""
    var language: String? = ""
}

class GoogleFetchController: ObservableObject{
    @Published var storedBooks: [String: Book] = [:] // Dictionary to store books by title+author
    private var lastFetchTime: Date? = nil
    private var fetchedResults: [Book] = [] // stores the fetched results
    @Published var titleParts : [String] = []
    @Published var authorParts : [String] = []
    
    // TODO: LATER when querying make sure that the book that you get matches the language of the book you're fetching; this is not important as of 02/09/25 but will later
    func fetchBookInfo(for givenBook: EbookData, completion: @escaping ([Book]) -> Void) {
        
        let title = givenBook.bookTitle ?? ""
        let author = givenBook.author ?? ""
        var isbn = givenBook.isbn ?? ""

        titleParts = title.split(separator: " ").map { String($0) }
        authorParts = author.split(separator: " ").map { String($0) }

        let searchTypes: [(String, Bool, String)] = [
            ("isbn:\(isbn)", !isbn.isEmpty, "ISBN Search"),
            ("intitle:\(title)+inauthor:\(author)", true, "Full Title + Full Author"),
            ("intitle:\(title)", true, "Only Title"),
            ("inauthor:\(author)", true, "Only Author"),
        ]
        tryNextSearch(searchTypes, givenBook, [], index: 0, completion: completion)
    }

    func tryNextSearch(_ searchTypes: [(String, Bool, String)], _ givenBook: EbookData, _ potentialMatches: [Book], index: Int, completion: @escaping ([Book]) -> Void) {
        guard index < searchTypes.count else {
            print("index val: \(index) | searchTypes.count: \(searchTypes.count)")
            print("❌ No results found for \(givenBook.bookTitle ?? "") by \(givenBook.author ?? ""). Trying split searches...")
            trySingleAuthorAndTitleQueries(givenBook, authorParts, titleParts, completion: completion)
            return
        }

        let (query, shouldTry, searchType) = searchTypes[index]
        if !shouldTry {
            tryNextSearch(searchTypes, givenBook, potentialMatches, index: index + 1, completion: completion)
            return
        }

        let urlString = "https://www.googleapis.com/books/v1/volumes?q=\(query)".addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed) ?? ""
        print("🔍 Searching via \(searchType): \(query)")

        guard let url = URL(string: urlString) else {
            print("❌ Invalid URL")
            return
        }

        URLSession.shared.dataTask(with: url) { data, response, error in
            guard let data = data, error == nil else {
                print("⚠️ No data received or error occurred")
                self.tryNextSearch(searchTypes, givenBook, potentialMatches, index: index + 1, completion: completion)
                return
            }
            
            do {
                if let jsonDict = try JSONSerialization.jsonObject(with: data, options: []) as? [String: Any],
                   let items = jsonDict["items"] as? [[String: Any]] {
                    var updatedMatches = potentialMatches
                    print("potentialMatches val: \(potentialMatches)")

                    for item in items {
                        if let volumeInfo = item["volumeInfo"] as? [String: Any] {
                            let googleBook = self.parseBook(volumeInfo)
                            
                            print("🔎 Checking result: \(googleBook.title) by \(googleBook.authors)")

                            if self.didFindMatch(googleBook, givenBook) {
                                print("found a match \(googleBook.title) and \(givenBook.bookTitle)")
                                if updatedMatches.count < 10 {
                                    updatedMatches.append(googleBook)
                                } else {
                                    break
                                }
                            }
                        }
                    }

                    DispatchQueue.main.async {
                        if updatedMatches.isEmpty {
                            print("🔄 No matches found in \(searchType), trying next search...")
                            self.tryNextSearch(searchTypes, givenBook, updatedMatches, index: index + 1, completion: completion)
                        } else {
                            completion(updatedMatches)
                        }
                    }
                }
            } catch {
                print("❌ JSON Parsing Error: \(error)")
                self.tryNextSearch(searchTypes, givenBook, potentialMatches, index: index + 1, completion: completion)
            }
        }.resume()
    }
    
    
    func didFindMatch(_ bookFromGoogle: Book, _ givenBook: EbookData)-> Bool
    {
        print("Comparing \(bookFromGoogle.title) to \(givenBook.bookTitle)")
        let titleKeywords = extractKeywords(from: givenBook.bookTitle ?? "")
        let authorKeywords = extractKeywords(from: givenBook.author ?? "")
        
        print("pagecount: \(bookFromGoogle.pageCount)")
        if bookFromGoogle.pageCount == 0 {
            print("⚠️ Page Count = 0. Skipping...")
            return false
        }
        
        // Check if the match is valid
        // TODO: Is still adding the result of this to
        if languageCleaner(bookFromGoogle.language ?? "") != languageCleaner(givenBook.language ?? "") {
            print("fetched book lang: \(languageCleaner(bookFromGoogle.language ?? "")) | given book lang \(languageCleaner(givenBook.language ?? ""))")
            print("⚠️ Skipping due to language mismatch")
            return false// Skip if language mismatches
        }
        
        if !self.authorMatches(bookFromGoogle.authors, authorKeywords) {
            print("⚠️ Skipping due to author mismatch")
            return false// Skip if author mismatch
        }
        
        if !self.titleMatches(bookFromGoogle.title, titleKeywords) {
            print("⚠️ Skipping due to title mismatch")
            return false// Skip if title mismatch
        }
        print("passed and this book is going to be added")
        return true
    }
    
    func parseBook(_ volumeInfo: [String: Any]) -> Book {
        return Book(
            title: volumeInfo["title"] as? String ?? "Unknown Title",
            authors: (volumeInfo["authors"] as? [String] ?? ["Unknown Author"]).joined(separator: ", "),
            publishedDate: volumeInfo["publishedDate"] as? String ?? "",
            pageCount: volumeInfo["pageCount"] as? Int64 ?? 0,
            bookSynopsis: volumeInfo["description"] as? String ?? "",
            isbn13: (volumeInfo["industryIdentifiers"] as? [[String: String]])?
                .first(where: { $0["type"] == "ISBN_13" })?["identifier"] ?? "",
            genre: (volumeInfo["categories"] as? [String])?.first,
            language: volumeInfo["language"] as? String ?? ""
        )
    }
    
    func cleanString(_ input: String) -> String {
        let unwantedCharacters: CharacterSet = CharacterSet.punctuationCharacters//.union(.whitespacesAndNewlines)
        let cleanedString = input.lowercased().components(separatedBy: unwantedCharacters)
            .joined()
        return cleanedString // Removes all punctuation and spaces
    }

    
    // counting this as a title mismatch : "cleanedGivenTitle: assassins blade | cleanedFetchedTitle the assassins blade"
    func titleMatches(_ fetchedTitle: String, _ keywords: [String]) -> Bool {
        // Clean both titles
        let cleanedFetchedTitle = cleanString(fetchedTitle)
        let cleanedGivenTitle = cleanString(keywords.joined(separator: " "))  // Rebuild the original given title from the keywords
        // Check if the cleaned fetched title is a substring of the cleaned given title
        if cleanedGivenTitle.contains(cleanedFetchedTitle) {
            return true
        }
        else
        {
            // Optional: If you want to check the presence of the keywords in the given title
            let titleWords = cleanedFetchedTitle.split(separator: " ").map { String($0) }
            return titleWords.allSatisfy { word in cleanedGivenTitle.contains(word) }
        }

    }

    
    func authorMatches(_ fetchedAuthor: String, _ keywords: [String]) -> Bool {
        let cleanedFetchedAuthor = cleanString(fetchedAuthor)
        let cleanedKeywords = keywords.map { cleanString($0) }
        
        let authorWords = cleanedFetchedAuthor.split(separator: " ").map { String($0) }
        return cleanedKeywords.allSatisfy { word in
            authorWords.contains(word)}
    }
    
//    func authorMatches(_ fetchedAuthor: String, _ keywords: [String]) -> Bool {
//        let authorWords = fetchedAuthor.lowercased().split(separator: " ").map { String($0) }
//            .filter { !$0.isEmpty && !$0.contains(where: { !$0.isLetter })}
//        //print("in authorMatches checking author words \(authorWords)")
//        //print("keywords: \(keywords)")
//        return keywords.allSatisfy { word in authorWords.contains(word) }
//    }
    
    private func trySingleAuthorAndTitleQueries(_ givenBook: EbookData, _ authorParts: [String], _ titleParts: [String], completion: @escaping ([Book]) -> Void) {
        var searchCombinations: [(String, String)] = []
        var potentialMatches: [Book] = []
        
        for author in authorParts {
            searchCombinations.append((givenBook.bookTitle ?? "", author)) // Full title + single author
            print("[full title + single author]:  \(givenBook.bookTitle) + \(author)")
        }
        for title in titleParts {
            searchCombinations.append((title, givenBook.author ?? "")) // Single title + full author
            print("[single title + full author]:  \(title) + \(givenBook.author)")
        }
        for author in authorParts {
            for title in titleParts {
                searchCombinations.append((title, author)) // Single title + single author
                print("[single title + single author]:  \(title) + \(author)")
            }
        }
        
        var index = 0
        print("search combinations: \(searchCombinations.count)")
        func attemptNext() {
            if index >= searchCombinations.count {
                print("❌ No results found with split title/author searches.")
                completion([])
                return
            }
            
            let (titleQuery, authorQuery) = searchCombinations[index]
            print("searchComboVal: \(searchCombinations[index])")
            index += 1
            
            print("🔍 Retrying with: Title - \(titleQuery), Author - \(authorQuery)")
            
            let query = "intitle:\(titleQuery)+inauthor:\(authorQuery)"
            let urlString = "https://www.googleapis.com/books/v1/volumes?q=\(query)".addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed) ?? ""
            
            print("url for split: \(urlString)")
            
            guard let url = URL(string: urlString) else {
                attemptNext()
                return
            }
            
            URLSession.shared.dataTask(with: url) { data, response, error in
                guard let data = data, error == nil else {
                    attemptNext()
                    return
                }
                
                do {
                    if let jsonDict = try JSONSerialization.jsonObject(with: data, options: []) as? [String: Any],
                       let items = jsonDict["items"] as? [[String: Any]] {
                        let books = items.compactMap { self.parseBook($0["volumeInfo"] as? [String: Any] ?? [:]) }
                        if books.isEmpty {
                            attemptNext()
                        } else {
                            DispatchQueue.main.async {
                                for book in books
                                {
                                    if self.didFindMatch(book, givenBook)
                                    {
                                        print("found a match from split: \(book)")
                                        potentialMatches.append(book)
                                    }
                                    else{
                                        attemptNext()
                                    }
                                }
                            }
                        }
                    } else {
                        attemptNext()
                    }
                } catch {
                    attemptNext()
                }
                completion(potentialMatches)
            }.resume()
        }
        attemptNext()
    }
    
    func extractKeywords(from text: String) -> [String] {
        let commonWords: Set<String> = ["a", "an", "and", "in", "on", "to", "for", "with", "at", "by"]
        // Stopwords to remove
        let words = text.lowercased()
            .components(separatedBy: CharacterSet.alphanumerics.inverted) // Split by non-alphanumeric characters
            .filter { !$0.isEmpty && !$0.contains(where: { !$0.isLetter }) && !commonWords.contains($0) } // Remove non-alphabetic words and stopwords
        //return words.joined(separator: "+") // Convert back to a search query format for API
        return words
    }
    
//    private func generateKey(title: String, author: String) -> String {
//        return "\(title.lowercased())-\(author.lowercased())"
//    }
    
//    func getStoredBook(title: String, author: String) -> Book? {
//        let key = generateKey(title: title, author: author)
//        return storedBooks[key]
//    }
    
    private func printBookInfo(book: Book) {
        print("""
        Title: \(book.title)
        Authors: \(book.authors)
        Published Date: \(book.publishedDate)
        Page Count: \(book.pageCount)
        Description: \(book.bookSynopsis)
        ISBN-13: \(book.isbn13)
        Genre: \(book.genre ?? "Unknown")
        """)
    }
    
    // Function to check if the title contains most of the extracted keywords
    func checkFetchedForKeywords(_ fetchedString: String, keywords: String) -> Bool {
        let keywordList = keywords.split(separator: "+").map { String($0) }
        let fetchedLowercase = fetchedString.lowercased()
        
        print("Match? Keywords: \(keywordList.joined(separator: ", ")) | Fetched: \(fetchedLowercase)")
        
        // Count the number of matching keywords in the title
        let matchCount = keywordList.filter { fetchedLowercase.contains($0.lowercased()) }.count
        
        // You can adjust this threshold based on how strict the match should be
        let threshold = keywordList.count / 2  // For example, at least half of the keywords should match
        return matchCount >= threshold
    }
    
    func languageCleaner(_ languageIn: String) -> String{
        let loweredLanguageIn = languageIn.lowercased()
        if loweredLanguageIn == "en" || loweredLanguageIn == "en-us"
        {
            return "English"
        }
        else
        {
            return languageIn
        }
    }
    
    func authorCleaner(authorIn: String) -> String{
        let normalizedAuthor = authorIn.folding(options: .diacriticInsensitive, locale: .current)
        print("Cleaned Author from \(authorIn) -> \(normalizedAuthor)")
        return normalizedAuthor
    }
    
    func editBookWithGoogleInfo(_ givenBook: EbookData, _ fetchedFromGoogleBook: Book) {
        //        fetchBookInfo(for: givenBook) { [weak self] fetchedFromGoogleBooks in
        //            guard let self = self, let fetchedFromGoogleBook = fetchedFromGoogleBooks.first else {
        //                print("No book fetched from Google to update Core Data.")
        //                return
        //            }
        let title = givenBook.bookTitle ?? ""
        let author = givenBook.author ?? ""
        let isbn = givenBook.isbn ?? ""
        let ebookFetchRequest: NSFetchRequest<EbookData> = EbookData.fetchRequest()
        
        DispatchQueue.main.async {
            if isbn.isEmpty {
                print("Fetching based on title and author")
                ebookFetchRequest.predicate = NSPredicate(format: "bookTitle == %@ AND author == %@", title, author)
            } else {
                print("Fetching based on isbn...")
                ebookFetchRequest.predicate = NSPredicate(format: "isbn == %@", isbn)
            }
            
            do {
                let existingBooks = try DataController.shared.container.viewContext.fetch(ebookFetchRequest)
                
                if let bookCD = existingBooks.first {
                    if bookCD.bookTitle != fetchedFromGoogleBook.title {
                        bookCD.bookTitle = fetchedFromGoogleBook.title
                    }
                    if bookCD.synopsis != fetchedFromGoogleBook.bookSynopsis {
                        bookCD.synopsis = fetchedFromGoogleBook.bookSynopsis
                    }
                    if bookCD.pageCount != fetchedFromGoogleBook.pageCount {
                        bookCD.pageCount = fetchedFromGoogleBook.pageCount
                    }
                    if bookCD.isbn?.isEmpty ?? true {
                        bookCD.isbn = fetchedFromGoogleBook.isbn13
                    }
                    try DataController.shared.container.viewContext.save()
                    print("✅ Updated Core Data with Google Book info: \(fetchedFromGoogleBook.title)")
                } else {
                    print("⚠️ No matching book found in Core Data.")
                }
            } catch {
                print("❌ Error fetching from Core Data: \(error.localizedDescription)")
            }
        }
    }
}
// try to find by ISBN first if that brings back nothing, fetch by title and author

