//
//  BookHarbourApp.swift
//  BookHarbour
//
//  Created by Summer Gasaway on 12/28/24.
//

import SwiftUI

@main
struct BookHarbourApp: App {
    var body: some Scene {
        WindowGroup {
            MainView(someBindingValue: .constant("Example"))
        }
    }
}
