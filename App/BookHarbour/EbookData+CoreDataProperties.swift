//
//  EbookData+CoreDataProperties.swift
//  
//
//  Created by Summer Gasaway on 2/9/25.
//
//

import Foundation
import CoreData


extension EbookData {

    @nonobjc public class func fetchRequest() -> NSFetchRequest<EbookData> {
        return NSFetchRequest<EbookData>(entityName: "EbookData")
    }

    @NSManaged public var author: String
    @NSManaged public var bookTitle: String
    @NSManaged public var coverImgPath: String?
    @NSManaged public var epubBookmark: BinaryData?
    @NSManaged public var isbn: Int16
    @NSManaged public var pageCount: Int16
    @NSManaged public var uuid: UUID?
    @NSManaged public var locator: BinaryData?  // Stores the last reading position


}
