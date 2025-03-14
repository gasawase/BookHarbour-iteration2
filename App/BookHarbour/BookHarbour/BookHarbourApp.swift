//
//  BookHarbourApp.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 12/28/24.
//

import SwiftUI

@main
struct BookHarbourApp: App {
    let dataController = DataController.shared

    var body: some Scene {
        WindowGroup {
            MainView()
                .environment(\.managedObjectContext, dataController.container.viewContext)
        }
    }
}
