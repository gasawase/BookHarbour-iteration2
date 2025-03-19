//
//  FileController.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 12/29/24.
//

import Foundation
import SwiftUI
import ReadiumShared
import ReadiumStreamer
import CoreData


class EpubManager : ObservableObject
{
    @Published var folderURL: URL?
    @Published var epubFiles: [URL] = []
    @Published var currentEpubIndex: Int = 0
    @Published var bookArr : [BookDetails] = []
    @Published var fileLocArr: [String] = []
    private let googleFetcher = GoogleFetchController()
    
    //var assetRetriever : AssetRetriever = AssetRetriever(httpClient: DefaultHTTPClient())
    var HTTPClient = DefaultHTTPClient()
    var assetRetriever = AssetRetriever(httpClient: DefaultHTTPClient())
    
    func getEpubData(from folderURL: URL, senderView: any View) async {
        // Ensure security-scoped access at the folder level
        guard folderURL.startAccessingSecurityScopedResource() else {
            print("Failed to access security-scoped resource for folder: \(folderURL)")
            return
        }
        
        defer {
            folderURL.stopAccessingSecurityScopedResource() // Ensures cleanup
        }

        var publicationOpener = PublicationOpener(
            parser: DefaultPublicationParser(
                httpClient: HTTPClient,
                assetRetriever: assetRetriever,
                pdfFactory: DefaultPDFDocumentFactory()
            )
        )
        

        do {
            let files = try FileManager.default.contentsOfDirectory(at: folderURL, includingPropertiesForKeys: nil, options: [.skipsHiddenFiles])
            let epubFiles = files.filter { $0.pathExtension.lowercased() == "epub" }

            for epubFile in epubFiles {
                // Safely unwrap absoluteURL
                guard let absoluteURL = epubFile.absoluteURL else {
                    print("Failed to get absolute URL for \(epubFile)")
                    continue
                }
                // Retrieve an `Asset` to access the file content
                switch await assetRetriever.retrieve(url: absoluteURL) {
                case .success(let asset):
                    // Open a `Publication` from the `Asset`
                    switch await publicationOpener.open(asset: asset, allowUserInteraction: true, sender: senderView) {
                    case .success(let publication):
                        let validIsbnVal = isbnCleaner(publication: publication)
                        guard let bookmarkData = try? epubFile.absoluteURL.bookmarkData(
                            options: .withoutImplicitSecurityScope,
                            includingResourceValuesForKeys: nil,
                            relativeTo: nil
                        ) else {
                            print("❌ Failed to create security-scoped bookmark for:", epubFile.absoluteURL)
                            return
                        }
//                        let epubLocator = Locator(href: publication.readingOrder.first?.href,
//                                                  mediaType: publication.readingOrder.first?.mediaType ?? MediaType.epub)
                        
                        // add a book to the array
                        let book = BookDetails(
                            _bookTitle: cleanTitle(publication.metadata.title ?? "Unknown Title"),
                            _bookAuthor: cleanAuthorName(publication.metadata.authors.first?.name ?? "Unknown Author"),
                            _pageCount: Int64(publication.metadata.numberOfPages ?? 0),
                            //_epubPath: absoluteURL.string,
                            _epubBookmark: bookmarkData,
                            _ISBN: validIsbnVal,
                            _coverPath: getCoverURL(manifestJSON: publication.manifest.json),
                            _language: publication.metadata.language?.description ?? "",
                            _locator: defaultLocator(for: publication)
                        )
                        //print("manifest.resources.json \(publication.manifest.resources)")
                        print("Title: \(book._bookTitle) | ISBN: \(book._ISBN) | Author: \(book._bookAuthor) | Language: \(book._language)")

                        DispatchQueue.main.async {
                            self.bookArr.append(book)
                            self.fileLocArr.append(absoluteURL.string)
                            self.addToCoreData(bookInstance: book)
                        }
                    case .failure(let error):
                        print("Failed to parse EPUB: \(error)")
                    }
                case .failure(let error):
                    print("Failed to retrieve asset: \(error)")
                }
            }
        } catch {
            print("Error loading EPUB files: \(error)")
        }
    }
    
    func isbnCleaner(publication: Publication) -> String
    {
        guard let identifierVal = publication.metadata.identifier, !identifierVal.isEmpty else{
            return ""
        }
        if identifierVal.unicodeScalars.contains(where: CharacterSet.letters.contains)
        {
            print("\(publication.metadata.title ?? "Unknown Title") | \(identifierVal.unicodeScalars.contains(where: CharacterSet.letters.contains))")
            return ""
        }
        else
        {
            print(identifierVal)
            return identifierVal
        }
    }
    
    func cleanTitle(_ title: String) -> String {
        return title.filter { $0.isLetter || $0.isWhitespace }
    }
    
    func cleanAuthorName(_ author: String) -> String {
        // Remove numbers
        let cleanedAuthor = author.filter { $0.isLetter || $0.isWhitespace || $0 == "," }
        
        // Check if the name is in "LastName, FirstName" format
        let components = cleanedAuthor.split(separator: ",", maxSplits: 1).map { $0.trimmingCharacters(in: .whitespaces) }
        
        if components.count == 2 {
            return "\(components[1]) \(components[0])" // Swap to "FirstName LastName"
        }
        
        return cleanedAuthor // Return as-is if there's no comma
    }
    
    // probably can change this to create and return a Manifest object
    func getCoverURL(manifestJSON: Dictionary<String, any Sendable>) -> String{
        return ""
//        if let coverHref = manifestItems[metaContentVal], coverHref.hasSuffix(".jpg") || coverHref.hasSuffix(".jpeg") || coverHref.hasSuffix(".png"){
//            coverImagePath = coverHref
//        }
//        
//        //let parentURL = opfURL
//        let parentFilePath = opfURL.deletingLastPathComponent()
//        let finalCoverURLPath = parentFilePath.appending(path: coverImagePath).path()
//        
//        return finalCoverURLPath
    }
    
    func addToCoreData(bookInstance: BookDetails)
    {
        let ebookFetchRequest: NSFetchRequest<EbookData> = EbookData.fetchRequest()
        ebookFetchRequest.predicate = NSPredicate(format: "bookTitle == %@ AND author == %@", bookInstance._bookTitle, bookInstance._bookAuthor)
        do{
            let existingBooks = try DataController.shared.container.viewContext.fetch(ebookFetchRequest)
            if let existingBook = existingBooks.first {
                print("Duplicate book found with title: \(existingBook.bookTitle ?? ""), author: \(existingBook.author ?? "")")
                return
            }
            else{

                let newAddBook = EbookData(context: DataController.shared.container.viewContext)
                newAddBook.bookTitle = bookInstance._bookTitle.isEmpty ? "Unknown Title" : bookInstance._bookTitle
                newAddBook.author = bookInstance._bookAuthor.isEmpty ? "Unknown Author" : bookInstance._bookAuthor
                newAddBook.isbn = bookInstance._ISBN
                newAddBook.epubBookmark = bookInstance._epubBookmark
                newAddBook.pageCount = bookInstance._pageCount
                newAddBook.coverImgPath = bookInstance._coverPath
                newAddBook.language = bookInstance._language
                newAddBook.locator = bookInstance._locator
            }
            try DataController.shared.container.viewContext.save()
        } catch let error as NSError {
            print("Error saving to CoreData: \(error.localizedDescription)")
        }
    }
    
    func defaultLocator(for publication: Publication) -> Data {
        let firstHref = publication.readingOrder.first?.href ?? ""

        guard let validHref = AnyURL(string: firstHref) else {
            fatalError("Invalid EPUB resource path: \(firstHref)")
        }
        
        let newLocator = Locator(
            href: validHref,  // Ensure `href` is a valid AnyURL
            mediaType: publication.readingOrder.first?.mediaType ?? .epub,
            title: publication.metadata.title,
            locations: Locator.Locations(progression: 0.0, totalProgression: 0.0, position: 1),
            text: Locator.Text() )
        guard let encodedLocator = UtilityClass.encodeLocator(newLocator) else
        {
            return Data()
        }
        return encodedLocator
    }

    
}
