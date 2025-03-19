//
//  Utility.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 3/14/25.
//

import Foundation
import ReadiumShared

class UtilityClass {
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
    static func encodeLocator(_ locator: Locator) -> Data? {
        do {
            let jsonData = try JSONSerialization.data(withJSONObject: locator.json, options: [])
            return jsonData
        } catch {
            print("Failed to encode Locator: \(error)")
            return nil
        }
    }
    
    func decodeLocator(from data: Data) -> Locator? {
        do {
            let jsonObject = try JSONSerialization.jsonObject(with: data, options: [])
            return try Locator(json: jsonObject)
        } catch {
            print("Failed to decode Locator: \(error)")
            return nil
        }
    }


}

