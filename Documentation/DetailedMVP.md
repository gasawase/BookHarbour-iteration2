# Detailed MVP
_______________________

## 1. Epub Reader Functionalty
- [ ] File Import
- [ ] File Parsing (speciically Epub Parsing)
- [ ] File Storage


## 2. Displaying Epub Content
- [ ] Reader UI
  - [ ] Pagination or scrolling
  - [ ] Font size, font, and background color customization
  - [ ] Bookmarking and navigation
  - [ ] Actual displaying of the book
- [ ] Progress Tracking


## 3. Metadata Management
- [ ] Metadata extraction
- [ ] Metadata display (display book info to user)
- [ ] Allow users to add custom tags, ratings, or reviews to book save data
- [ ] Connect to Google API to retrieve book information that isn't included in the original Epub metadata
- [ ] Allow users to add custom book wraps or just book covers and spines

## 4. Bookshelf Functionality
- [ ] a. Expandable Panel for Book Selection
  - [ ] Create scrollable/expandable UI panel showing a list of imported books
  - [ ] Functionality for:
    - [ ] Search, filter, and sort books by metadata
    - [ ] Drag-and-drop from panel to 3D shelf
- [ ] b. 3D Bookshelf
  - [ ] Book placement
    - [X] Drag-and-drop functionality for placing books onto the bookshelf
    - [X] Define snap zones on the bookshelf where books can be placed
  - [ ] Reordering
    - [ ] Allow users to drag books already on the shelf to reposition them
    - [ ] Handle collision detection to prevent overlapping books
- [ ] c. Bookshelf Editing
  - [ ] Provide tools for users to:
    - [ ] Save and load bookshelf layouts
    - [ ] Name and rename bookshelves
- [ ] d. Visual Effects
  - [ ] Add decorations like lighting, rugs, and other aesthetic factors
  - [ ] Animation effects for smooth drag-and-drop interactions   

## 5. Drag and Drop Functionality
- [ ] a. Interaction Handling
  - [X] Implement gestures for dragging (touch, mouse)
  - [X] Define boundaries for valid drop zones
  - [ ] Provide visual feedback during dragging (e.g., highlight valid zones, show an outline of the book being dragged)
  - [ ] Return book to book list when dragged back to panel or otherwise removed
  - [ ] Remove book from book list when dragged onto shelf
- [ ] b. Snap-To-Grid System
  - [X] When a book is dropped, snap to nearest slot
- [ ] c. Undo/Redo System
  - [ ] Allow users to undo/redo actions 

## 6. Data Persistence
- [ ] a. Bookshelf State
  - [ ] Save the state of the bookshelf including:
    - [ ] ID of books placed on shelves (their positions will be saved in their data information)
    - [ ] Bookshelf's decorations and its name
  - [ ] Ensure this data is relocated correctly when the app loads
- [ ] b. User Preferences
  - [ ] Store user preferences for the reader (e.g., font size, theme) and bookshelf designs

## 7. Integration Between Components
- [ ] a. Linking between EpubReader and the Unity Bookshelf manager
  - [ ] Ensure books placed on 3D bookshelf can be opened directly in the reader
  - [ ] Maintain synchronization between the metadata in the reader and the bookshelf (e.g., book title, book progress?)
- [ ] b. Unity and Swift Integration
  - [X] Use Unity for rendering the 3D bookshelf and manage interactions like drag-and-drop, snapping, and custom decorating
  - [ ] Integrate Unity as a Library into the SwiftUI based iOS app 

## 8. User Interface (UI) Design
- [ ] a. Main App Layout
  - [ ] Includes sections for:
    - [ ] Epub library (file management)
    - [ ] EpubReader
    - [ ] Expandable panel for book section
    - [ ] 3D bookshelf editor
- [ ] b. Navigation
  - [ ] Provide way to navigate between different app sections (e.g., tabs)
- [ ] c. Settings Menu
  - [ ] Add options for:
    - [ ] Reader Settings (fonts, themes, etc.)
    - [ ] Bookshelf Customization
    - [ ] General App Settings (import paths, storage limits, etc.)

## 9. Performance Optimization
- [ ] a. Handling Large Libraries
  - [ ] Optimize for large book collections for smooth scrolling and loading
  - [ ] Use lazy loading to only render visible books
- [ ] b. Resource Management
  - [ ] Efficiently handle textures, 3D models, and metadata to minimize memory storage
  - [ ] Asynchronously load book covers and content to avoid blocking the UI 

## 10. Testing and Error Handling
- [ ] a. Error Handling
  - [ ] Invalid Epub files
  - [ ] Missing metadata
  - [ ] Failed drag-and-drop operations
- [ ] b. User Feedback
  - [ ] Feedback to users regarding errors
- [ ] c. Unit Testing
  - [ ] Test individual features, like drag-and-drop and metadata parsing
  - [ ] Ensure proper integration between reader and bookshelf 



