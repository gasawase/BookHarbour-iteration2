//
//  ReaderController.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 3/19/25.
//

import Foundation
import SwiftUI
import UIKit
import ReadiumShared
import ReadiumStreamer
import ReadiumNavigator
import ReadiumAdapterGCDWebServer

class ReaderController : ObservableObject
{
    var HTTPClient = DefaultHTTPClient()
    var assetRetriever = AssetRetriever(httpClient: DefaultHTTPClient())
    var utilityClass = UtilityClass()

    public func convertBookmarkToURL(from bookmark: Data?) async -> Publication? {
        
        if let storedBookmark = bookmark {
            var isStale = false
            do {
                let resolvedURL = try URL(resolvingBookmarkData: storedBookmark, options: .withoutImplicitStartAccessing, bookmarkDataIsStale: &isStale)

                if isStale {
                    print("⚠️ Bookmark is stale. Need to re-save it.")
                }

                // ✅ Start security-scoped access
                guard resolvedURL.startAccessingSecurityScopedResource() else {
                    print("❌ Failed to access security-scoped resource.")
                    return nil
                }
                defer { resolvedURL.stopAccessingSecurityScopedResource() }

                // ✅ Now you can safely use the resolvedURL
                print("✅ Successfully accessed EPUB file:", resolvedURL)
                let publication = await readBook(for: resolvedURL)
                return publication

            } catch {
                print("❌ Failed to resolve bookmark:", error)
            }
        }
        return nil
    }
    
    public func readBook(for url:URL) async -> Publication?
    {
        guard let absoluteURL = FileURL(url: url) else {
            print("Failed to retrieve absolute url: \(url)")
            return nil
        }
        
        print("url is \(absoluteURL)")
        
        var publicationOpener = PublicationOpener(
            parser: DefaultPublicationParser(
                httpClient: HTTPClient,
                assetRetriever: assetRetriever,
                pdfFactory: DefaultPDFDocumentFactory()
            )
        )

        switch await assetRetriever.retrieve(url: absoluteURL) {
        case .success(let asset):
            switch await publicationOpener.open(asset: asset, allowUserInteraction: true, sender: nil) {
            case .success(let publication):
                print("Created a publication!")
                return publication
            case .failure(let error):
                print("Failed to open EPUB: \(error)")
                return nil
            }
        case .failure(let error):
            print("Failed to retrieve asset: \(error)")
            return nil
        }
    }
    
    public func readBook(for book: EbookData) async
    {
//        guard let bookPath = book.epubPath else {
//            print("Book path is invalid: \(book.epubPath)")
//            return
//        }
//        print("Book Path: \(bookPath)")
//        guard let bookBookmark = book.epubBookmark else {
//            print("Book bookmark is invalid: \(book.epubBookmark)")
//            return
//        }
        guard let ebookPublicationObj = await convertBookmarkToURL(from: book.epubBookmark) else {return}
        
        if ebookPublicationObj.conforms(to: .epub)
        {
            guard let decodedLocator = utilityClass.decodeLocator(from: book.locator ?? Data()) else
            {
                print("Failed to decode Locator")
                return
            }
            do {
                let navigator = try await EPUBNavigatorViewController(publication: ebookPublicationObj, initialLocation: nil, httpServer: GCDHTTPServer(assetRetriever: assetRetriever))
            } catch {
                print("Failed to create a navigator")
                return
            }
        }
    }
}
