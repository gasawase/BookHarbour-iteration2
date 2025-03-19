//
//  BookDetails.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 12/31/24.
//

import Foundation
import SwiftUI
import ReadiumShared
import ReadiumStreamer

// the Book information that we will be storing; all of these have default information so nothing comes up null
struct BookDetails : Identifiable, Hashable
{
    var id: UUID = UUID()
    
    var _bookTitle: String = ""
    var _bookAuthor: String = ""
    //var _currentChapter: Int = 0
    var _pageCount: Int64 = 0
    var _epubBookmark: Data = Data()
    var _ISBN: String = ""
    var _coverPath: String = ""
    var _language: String = ""
//    @Published var bookOPFPath : String = ""
//    @Published var bookOPFURL : URL = URL(fileURLWithPath: "/path/to/file.txt")
//    @Published var readingProgressSeconds : Int = 0
//    @Published var manifestDictionary : [String: String] = [:]
//    @Published var bookEpubPath : String = ""
    //var chaptersTotal: Int = 0
    // current location in book
    
    // Getters
    func getBookTitle() -> String {
        return _bookTitle
    }
    
    func getBookAuthor() -> String {
        return _bookAuthor
    }
    
//    func getCurrentChapter() -> Int {
//        return _currentChapter
//    }
    
    func getPageCount() -> Int64 {
        return _pageCount
    }
    
    func getEpubBookmark() -> Data {
        return _epubBookmark
    }
    
    func getISBN() -> String {
        return _ISBN
    }
    
    // Setters
    mutating func setBookTitle(_ title: String) {
        _bookTitle = title
    }
    
    mutating func setBookAuthor(_ author: String) {
        _bookAuthor = author
    }
    
//    mutating func setCurrentChapter(_ chapter: Int) {
//        _currentChapter = chapter
//    }
    
    mutating func setPageCount(_ count: Int64) {
        _pageCount = count
    }
    
    mutating func setEpubBookmark(_ data: Data) {
        _epubBookmark = data
    }
    
    mutating func setISBN(_ isbn: String) {
        _ISBN = isbn
    }
}
