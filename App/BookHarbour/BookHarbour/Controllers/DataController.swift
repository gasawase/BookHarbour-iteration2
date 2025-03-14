//
//  DataController.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 2/6/25.
//

import Foundation
import CoreData
import SwiftUI

struct DataController{
    static let shared = DataController()
    let container: NSPersistentContainer
    
    init(inMemory: Bool = false) {
        // If you didn't name your model Main you'll need
        // to change this name below.
        container = NSPersistentContainer(name: "CoreData")
        
        if inMemory {
            container.persistentStoreDescriptions.first?.url = URL(fileURLWithPath: "/dev/null")
        }
        
        container.loadPersistentStores { description, error in
            if let error = error {
                fatalError("Error: \(error.localizedDescription)")
            }
        }
    }
    
    func save(){
        let context = container.viewContext

        if context.hasChanges {
            do {
                try context.save()
                print("saved")
            } catch {
                print("Could not save")
            }
        }
    }
    
    func deleteBookEntry(ebook: EbookData){
        container.viewContext.delete(ebook)
        do{
            try container.viewContext.save()
        } catch{
            container.viewContext.rollback()
            print("Failed to save context \(error)")
        }
    }
    
    func getAllTitles() -> [EbookData]{
        let fetchRequest : NSFetchRequest<EbookData> = EbookData.fetchRequest()
        do{
            return try container.viewContext.fetch(fetchRequest)
        } catch{
            return[]
        }
    }
    
    func clearAllTitles(){
        let allTitles = getAllTitles()
        
        for book in allTitles{
            deleteBookEntry(ebook: book)
        }
    }
    
}
