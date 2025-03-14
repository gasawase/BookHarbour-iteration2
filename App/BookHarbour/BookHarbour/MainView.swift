import SwiftUI

// MARK: - [ViewName] View
struct MainView: View {
    // MARK: - Properties
    @State private var isSomethingActive: Bool = false

    
    var body: some View {
        EpubPickerView()
    }

}

// MARK: - Preview
struct MainView_Previews: PreviewProvider {
    static var previews: some View {
        NavigationView {
            MainView()
        }
    }
}
