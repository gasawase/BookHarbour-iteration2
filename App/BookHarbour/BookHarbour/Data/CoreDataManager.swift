//
//  CoreDataManager.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 3/14/25.
//

import Foundation
import CoreData
class CoreDataManager {
    static let shared = CoreDataManager()
    private init() {}

    // Adds a new book to Core Data if it doesn't already exist
    func addBookToCoreData(bookInstance: BookDetails) {
        let ebookFetchRequest: NSFetchRequest<EbookData> = EbookData.fetchRequest()
        ebookFetchRequest.predicate = NSPredicate(format: "bookTitle == %@ AND author == %@", bookInstance._bookTitle, bookInstance._bookAuthor)
        do {
            let existingBooks = try DataController.shared.container.viewContext.fetch(ebookFetchRequest)
            if let existingBook = existingBooks.first {
                print("Duplicate book found: \(existingBook.bookTitle ?? "")")
                return
            } else {
                let newBook = EbookData(context: DataController.shared.container.viewContext)
                newBook.bookTitle = bookInstance._bookTitle
                newBook.author = bookInstance._bookAuthor
                newBook.isbn = bookInstance._ISBN
                newBook.epubBookmark = bookInstance._epubBookmark
                newBook.pageCount = bookInstance._pageCount
                newBook.coverImgPath = bookInstance._coverPath
                newBook.language = bookInstance._language
                try DataController.shared.container.viewContext.save()
            }
        } catch {
            print("❌ Error saving book to Core Data: \(error.localizedDescription)")
        }
    }
}
