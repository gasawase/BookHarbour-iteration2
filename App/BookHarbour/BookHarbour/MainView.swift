import SwiftUI

// MARK: - [ViewName] View
struct MainView: View {
    // MARK: - Properties
    @State private var isSomethingActive: Bool = false
    @Binding var someBindingValue: String // Example binding if needed
    
    var body: some View {
        VStack(spacing: 16) {
            // MARK: - Header Section
            Text("[Title of the View]")
                .font(.largeTitle)
                .fontWeight(.bold)
                .padding()
            
            // MARK: - Main Content
            Spacer()
            Text("Main content goes here")
                .font(.body)
            
            Button(action: {
                isSomethingActive.toggle()
            }) {
                Text(isSomethingActive ? "Deactivate" : "Activate")
                    .frame(maxWidth: .infinity)
                    .padding()
                    .background(Color.blue)
                    .foregroundColor(.white)
                    .cornerRadius(8)
            }
            .padding(.horizontal)
            
            // MARK: - Footer Section
            Spacer()
        }
        .padding()
        .background(Color(.systemBackground))
        .navigationTitle("[Navigation Title]")
        .toolbar {
            ToolbarItem(placement: .navigationBarTrailing) {
                Button(action: {
                    // Action for toolbar button
                }) {
                    Image(systemName: "gear")
                }
            }
        }
    }
}

// MARK: - Preview
struct MainView_Previews: PreviewProvider {
    static var previews: some View {
        NavigationView {
            MainView(someBindingValue: .constant("Example"))
        }
    }
}
