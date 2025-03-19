import SwiftUI
import CoreData

struct EpubPickerView: View {
    @ObservedObject var epubManager = EpubManager()
    //@StateObject var googleFetcher = GoogleFetchController()
    @ObservedObject var readerController = ReaderController()
    
    @State private var selectedBook: EbookData? = nil
    @State private var fetchedBook: Book? = nil
    @State private var showPopup = false
    @State private var showNoResultsPopup = false
    @State private var isFilePickerPresented = false
    @State private var selectedFolderURL: URL?
    @State private var matchedResults: [Book] = []
    @State private var currentMatchIndex: Int = 0
    @State private var nextClickCount = 0
    @State private var isFetchingMoreResults = false
    
    let dataController = DataController.shared
    
    @FetchRequest(sortDescriptors: [SortDescriptor(\.bookTitle)])
    var booksFromCoreData: FetchedResults<EbookData>
    
    var body: some View {
        NavigationStack {
            VStack {
                if epubManager.fileLocArr.isEmpty {
                    Text("No folder selected")
                } else {
                    BookListView()
                }
            }
            //.overlay { popupOverlay }
            .toolbar { toolbarContent }
            .fileImporter(isPresented: $isFilePickerPresented, allowedContentTypes: [.folder], allowsMultipleSelection: false, onCompletion: handleFileImport)
        }
    }
}

// MARK: - Subviews

private extension EpubPickerView {
    @ViewBuilder
    func BookListView() -> some View {
        List(booksFromCoreData) { book in
//            BookRow(book: book, onReplaceInfo: { fetchBookInfo(for: book) })
//            BookRow(book: book, pressedRead: { openBookToRead(for: book) })
            BookRow(book: book, pressedRead: {
                await readerController.readBook(for: book)
            })
                //.padding(.vertical, 4)
        }
    }
    
//    @ViewBuilder
//    var popupOverlay: some View {
//        if showPopup, let selected = selectedBook {
//            BookUpdatePopup(
//                originalBook: selected,
//                fetchedBook: fetchedBook ?? Book(),
//                onConfirm: confirmBookUpdate,
//                onNext: fetchNextBook
//            )
//        } else if showNoResultsPopup {
//            NoResultsPopup(onDismiss: { showNoResultsPopup = false })
//        }
//    }
    
    @ToolbarContentBuilder
    var toolbarContent: some ToolbarContent {
        ToolbarItemGroup(placement: .topBarLeading) {
            Button("Select a Folder...") {
                isFilePickerPresented = true
            }
            Button("Reset Books") {
                resetBooks()
            }
            Text("Number of books: \(booksFromCoreData.count)")
        }
    }
}

// MARK: - Actions

private extension EpubPickerView {
//    func fetchBookInfo(for book: EbookData) {
//        currentMatchIndex = 0
//        googleFetcher.fetchBookInfo(for: book) { bookArr in
//            if !bookArr.isEmpty {
//                matchedResults = bookArr
//                fetchedBook = matchedResults.first
//                selectedBook = book
//                showPopup = true
//            } else {
//                showNoResultsPopup = true
//            }
//        }
//    }
    
//    func fetchNextBook() {
//        if currentMatchIndex < matchedResults.count - 1 {
//            currentMatchIndex += 1
//            fetchedBook = matchedResults[currentMatchIndex]
//        } else {
//            attemptFetchingMoreResults()
//        }
//    }
    
//    func attemptFetchingMoreResults() {
//        nextClickCount += 1
//        if nextClickCount >= 3 {
//            isFetchingMoreResults = true
//            nextClickCount = 0
//            googleFetcher.fetchBookInfo(for: selectedBook!) { newBooks in
//                isFetchingMoreResults = false
//                matchedResults = newBooks
//                currentMatchIndex = 0
//                fetchedBook = matchedResults.first ?? Book()
//                showPopup = !matchedResults.isEmpty
//            }
//        } else {
//            showNoResultsPopup = true
//            showPopup = false
//        }
//    }
    
    func confirmBookUpdate() {
        //googleFetcher.editBookWithGoogleInfo(selectedBook!, fetchedBook ?? Book())
        matchedResults = []
        showPopup = false
    }
    
    func resetBooks() {
        dataController.clearAllTitles()
        epubManager.fileLocArr.removeAll()
        epubManager.bookArr.removeAll()
        //googleFetcher.storedBooks.removeAll()
    }
    
    func handleFileImport(result: Result<[URL], Error>) {
        switch result {
        case .success(let urls):
            if let url = urls.first {
                selectedFolderURL = url
                Task { await epubManager.getEpubData(from: url, senderView: self) }
            }
        case .failure(let error):
            print("Error selecting file: \(error.localizedDescription)")
        }
    }
}

// MARK: - Components

struct BookRow: View {
    let book: EbookData
    //let onReplaceInfo: () -> Void
    let pressedRead: () async -> Void
    
    var body: some View {
        HStack(alignment: .top) {
//            Button(action: onReplaceInfo) {
//                Text("Replace Info from Google")
//                    .padding(8)
//                    .background(Color.green)
//            }
//            .buttonStyle(BorderlessButtonStyle())
            
            VStack(alignment: .leading) {
                Text("📖 \(book.bookTitle ?? "Unknown Title")").font(.headline)
                Text("✍️ \(book.author ?? "Unknown Author")").font(.subheadline)
                Text("📝 Pages: \(book.pageCount)")
                Text("🔍 ISBN: \(book.isbn ?? "No ISBN")")
            }
            .frame(maxWidth: .infinity, alignment: .leading)
            
            Button(action: {
                Task{
                    await pressedRead()
                }
            }) {
                Text("Read")
                    .padding(8)
                    .background(Color.blue)
            }
        }
    }
}


struct BookUpdatePopup: View {
    var originalBook: EbookData
    var fetchedBook: Book
    var onConfirm: () -> Void
    var onNext: () -> Void
    
    var body: some View {
        VStack {
            Text("Google Book Info Found!").font(.headline)
            Divider()
            HStack {
                Spacer()
                VStack(alignment: .leading) {
                    Text("Original Title: \(originalBook.bookTitle ?? "Unknown")")
                    Text("Author: \(originalBook.author ?? "Unknown")")
                    Text("Pages: \(originalBook.pageCount)")
                }
                Spacer()
                VStack(alignment: .leading) {
                    Text("New Title: \(fetchedBook.title)")
                    Text("Author: \(fetchedBook.authors)")
                    Text("Pages: \(fetchedBook.pageCount)")
                }
                Spacer()
            }
            HStack {
                Spacer()
                Button("Confirm") { onConfirm() }
                Spacer()
                Button("Next") { onNext() }
                Spacer()
            }
        }
        .padding()
        .background(Color.white)
        .cornerRadius(10)
        .shadow(radius: 10)
    }
}

struct NoResultsPopup: View {
    var onDismiss: () -> Void
    
    var body: some View {
        VStack {
            Text("No Results Found")
                .font(.headline)
                .padding()
            
            Text("We couldn't find any book information from Google. Please try again later.")
                .multilineTextAlignment(.center)
                .padding()
            
            Button("Dismiss") {
                onDismiss()
            }
            .padding()
            .background(Color.red)
            .foregroundColor(.white)
            .cornerRadius(8)
        }
        .padding()
        .background(Color.white)
        .cornerRadius(10)
        .shadow(radius: 10)
    }
}


#Preview {
    EpubPickerView()
}
