//
//  NetworkManager.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 3/14/25.
//

import Foundation
class NetworkManager {
    static let shared = NetworkManager()
    private init() {}

    // Fetches books from the Google Books API for a given query
    func fetchBooks(query: String, completion: @escaping (Result<[Book], Error>) -> Void) {
        let urlString = "https://www.googleapis.com/books/v1/volumes?q=\(query)"
        guard let url = URL(string: urlString) else {
            completion(.failure(NetworkError.invalidURL))  // Return error if the URL is invalid
            return
        }

        // Make the network request
        URLSession.shared.dataTask(with: url) { data, response, error in
            if let error = error {
                completion(.failure(error))  // Return error if network request fails
                return
            }
            
            guard let data = data else {
                completion(.failure(NetworkError.noData))  // Return error if no data received
                return
            }

            do {
                // Parse the JSON response
                if let jsonDict = try JSONSerialization.jsonObject(with: data, options: []) as? [String: Any],
                   let items = jsonDict["items"] as? [[String: Any]] {
                    let books = items.compactMap { self.parseBook($0["volumeInfo"] as? [String: Any] ?? [:]) }
                    completion(.success(books))  // Return the parsed books
                } else {
                    completion(.failure(NetworkError.parsingError))  // Return error if parsing fails
                }
            } catch {
                completion(.failure(error))  // Return error if JSON parsing fails
            }
        }.resume()
    }

    // Parses the volume info to create a Book object
    private func parseBook(_ volumeInfo: [String: Any]) -> Book {
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
}

enum NetworkError: Error {
    case invalidURL, noData, parsingError  // Define custom network errors
}
