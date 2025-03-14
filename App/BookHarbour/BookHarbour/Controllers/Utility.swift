//
//  Utility.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 3/14/25.
//

import Foundation
class Utility {
    // Cleans a string by removing non-alphanumeric characters and converting to lowercase
    static func cleanString(_ input: String) -> String {
        return input.lowercased().filter { $0.isLetter || $0.isWhitespace }
    }
    
    // Extracts meaningful keywords from a text (e.g., removing common words)
    static func extractKeywords(from text: String) -> [String] {
        let commonWords: Set<String> = ["a", "an", "and", "in", "on", "to", "for", "with", "at", "by"]
        return text.lowercased().components(separatedBy: CharacterSet.alphanumerics.inverted)
            .filter { !$0.isEmpty && !commonWords.contains($0) }
    }
}

