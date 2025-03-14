//
//  MatchManager.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 3/14/25.
//

import Foundation
class MatchManager {
    
    // Check if a book from Google matches the given book data
    func isValidMatch(_ book: Book, givenBook: EbookData) -> Bool {
        let titleKeywords = extractKeywords(from: givenBook.bookTitle ?? "")
        let authorKeywords = extractKeywords(from: givenBook.author ?? "")
        
        // First, check title match - exact match or partial match
        if !titleMatches(book.title, keywords: titleKeywords) {
            return false
        }
        
        // Then check author match
        if !authorMatches(book.authors, keywords: authorKeywords) {
            return false
        }
        
        // Check language match
        if languageCleaner(book.language ?? "") != languageCleaner(givenBook.language ?? "") {
            return false
        }
        
        // Only consider books with valid page count
        return book.pageCount > 0
    }

    // Match book title with the keywords (exact match or partial)
    private func titleMatches(_ title: String, keywords: [String]) -> Bool {
        let cleanedTitle = cleanString(title)
        
        // Check if all keywords are present in the title
        let matchedKeywords = keywords.filter { cleanedTitle.contains(cleanString($0)) }
        print("Matching Words: \(matchedKeywords)" )
        
        // If we match at least half of the keywords, we can consider this a match
        return matchedKeywords.count >= (keywords.count / 2)
    }

    // Match book author with the keywords (exact match or partial)
    private func authorMatches(_ authors: String, keywords: [String]) -> Bool {
        let cleanedAuthors = cleanString(authors)
        return keywords.allSatisfy { cleanedAuthors.contains(cleanString($0)) }
    }

    // Clean a string (lowercase, remove non-alphanumeric characters)
    private func cleanString(_ input: String) -> String {
        return input.lowercased().filter { $0.isLetter || $0.isWhitespace }
    }

    // Extract meaningful keywords from a given text (ignores common words like "the", "a", etc.)
    private func extractKeywords(from text: String) -> [String] {
        let commonWords: Set<String> = ["a", "an", "and", "in", "on", "to", "for", "with", "at", "by"]
        return text.lowercased().components(separatedBy: CharacterSet.alphanumerics.inverted)
            .filter { !$0.isEmpty && !commonWords.contains($0) }
    }

    // Clean the language string for consistency
    private func languageCleaner(_ language: String) -> String {
        return language.lowercased() == "en" ? "English" : language
    }
}


