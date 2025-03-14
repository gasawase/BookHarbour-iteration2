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

class GoogleFetchController: ObservableObject {
    @Published var storedBooks: [String: Book] = [:]  // Stores fetched books by their title+author
    private var matchManager = MatchManager()  // Manages book matching logic
    
    // MARK: - Fetch Book Information
    // Initiates the search and fetches book information for the given book
    func fetchBookInfo(for givenBook: EbookData, completion: @escaping ([Book]) -> Void) {
        let queries = createSearchQueries(for: givenBook)  // Creates search queries based on book data
        performSearch(queries: queries, givenBook: givenBook, completion: completion)  // Performs the search
    }
    
    // MARK: - Helper Methods
    
    // Creates search queries for fetching books from the Google Books API
    private func createSearchQueries(for givenBook: EbookData) -> [(query: String, shouldTry: Bool)] {
        let title = givenBook.bookTitle ?? ""
        let author = givenBook.author ?? ""
        let isbn = givenBook.isbn ?? ""
        
        // Defines different search strategies based on the available book data
        return [
            ("isbn:\(isbn)", !isbn.isEmpty),
            ("intitle:\(title)+inauthor:\(author)", true),
            ("intitle:\(title)", true),
            ("inauthor:\(author)", true)
        ]
    }
    
    // Performs the search for books using the queries generated
    private func performSearch(queries: [(query: String, shouldTry: Bool)], givenBook: EbookData, completion: @escaping ([Book]) -> Void) {
        var potentialMatches: [Book] = []  // Holds the matched book results
        
        // Loops through the search queries and fetches books for each query
        for query in queries {
            if query.shouldTry {
                fetchBooks(forQuery: query.query) { books in
                    // Filter books using MatchManager after they are fetched
                    let matchedBooks = books.filter { self.matchManager.isValidMatch($0, givenBook: givenBook) }
                    
                    // Append only the valid matches to the potential matches list
                    potentialMatches.append(contentsOf: matchedBooks)
                    if !potentialMatches.isEmpty {
                        DispatchQueue.main.async {
                            completion(potentialMatches)  // Return matched books if found
                        }
                        return
                    }
                }
            }
        }
        
        // If no results found, try split searches
        trySplitSearch(givenBook, completion: completion)
    }
    
    // Fetches books from the Google Books API using a given query
    private func fetchBooks(forQuery query: String, completion: @escaping ([Book]) -> Void) {
        NetworkManager.shared.fetchBooks(query: query) { result in
            switch result {
            case .success(let books):
                DispatchQueue.main.async {
                    completion(books)  // Return fetched books
                }
            case .failure(let error):
                print("❌ Error fetching books: \(error.localizedDescription)")  // Log error if fetch fails
                DispatchQueue.main.async {
                    completion([])  // Return empty list on failure
                }
            }
        }
    }

    // If initial searches fail, perform split searches (e.g., title + one author part, etc.)
    private func trySplitSearch(_ givenBook: EbookData, completion: @escaping ([Book]) -> Void) {
        let authorParts = givenBook.author?.split(separator: " ").map { String($0) } ?? []
        let titleParts = givenBook.bookTitle?.split(separator: " ").map { String($0) } ?? []
        
        // Generate combinations of title and author parts for split searching
        var searchCombinations = generateSplitSearchCombinations(titleParts: titleParts, authorParts: authorParts)
        
        searchCombinations.forEach { searchCombo in
            fetchBooks(forQuery: "intitle:\(searchCombo.title)+inauthor:\(searchCombo.author)") { books in
                if !books.isEmpty {
                    DispatchQueue.main.async {
                        completion(books)  // Return books from split search if found
                    }
                }
            }
        }
    }

    // Generates combinations of title and author parts for split searches
    private func generateSplitSearchCombinations(titleParts: [String], authorParts: [String]) -> [(title: String, author: String)] {
        var combinations: [(String, String)] = []
        
        for author in authorParts {
            combinations.append((titleParts.joined(separator: " "), author)) // Full title + single author
        }
        
        for title in titleParts {
            combinations.append((title, authorParts.joined(separator: " "))) // Single title + full author
        }
        
        return combinations
    }
    
    func editBookWithGoogleInfo(_ givenBook: EbookData, _ fetchedFromGoogleBook: Book) {
        // Fetch the book from Core Data by title, author, or ISBN
        let ebookFetchRequest: NSFetchRequest<EbookData> = EbookData.fetchRequest()
        ebookFetchRequest.predicate = NSPredicate(format: "bookTitle == %@ AND author == %@", givenBook.bookTitle ?? "", givenBook.author ?? "")

        // Update the book with the new information
        do {
            let existingBooks = try DataController.shared.container.viewContext.fetch(ebookFetchRequest)
            if let bookCD = existingBooks.first {
                // Update book details
                if bookCD.bookTitle != fetchedFromGoogleBook.title {
                    bookCD.bookTitle = fetchedFromGoogleBook.title
                }
                if bookCD.author != fetchedFromGoogleBook.authors {
                    bookCD.author = fetchedFromGoogleBook.authors
                }
                if bookCD.pageCount != fetchedFromGoogleBook.pageCount {
                    bookCD.pageCount = fetchedFromGoogleBook.pageCount
                }
                if bookCD.synopsis != fetchedFromGoogleBook.bookSynopsis {
                    bookCD.synopsis = fetchedFromGoogleBook.bookSynopsis
                }
                if bookCD.isbn?.isEmpty ?? true {
                    bookCD.isbn = fetchedFromGoogleBook.isbn13
                }
                
                // Save the changes to Core Data
                try DataController.shared.container.viewContext.save()
            }
        } catch {
            print("❌ Error updating book in Core Data: \(error.localizedDescription)")
        }
    }

}
