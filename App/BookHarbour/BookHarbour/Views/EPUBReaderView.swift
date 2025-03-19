//
//  EPUBReaderView.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 3/19/25.
//

import Foundation
import SwiftUI
import UIKit
import ReadiumNavigator
import ReadiumShared

//struct EPUBReaderView: UIViewControllerRepresentable {
//    let publication: Publication
//    let book: EbookData
//    
//    func makeUIViewController(context: Context) -> EPUBViewController{
////        let httpServer = ReadiumShared.HTTPServer
//        
//        return try! EPUBViewController(
//            publication: publication,
//            locator: nil,
//            bookId: book.objectID.uriRepresentation().absoluteString,
//            books: DataController.shared,
//            bookmarks: BookmarkRepository(),
//            highlights: HighlightRepository(),
//            initialPreferences: EPUBPreferences(),
//            preferencesStore: UserDefaultsUserPreferencesStore(),
//            httpServer: httpServer
//        )
//    }
//    
//    func updateUIViewController(_ uiViewController: EPUBViewController, context: Context) {
//        // No updates needed for now
//    }
//}
