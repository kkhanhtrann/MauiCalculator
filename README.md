# MauiCalculator

This is a basic calculator application developed using .NET MAUI. It supports arithmetic operations such as addition, subtraction, multiplication, and division, and implements logic for parsing and evaluating mathematical formulas. The calculator is designed with a graphical user interface (GUI) and uses custom logic for evaluating expressions with support for variables.

## Features
- **Basic Arithmetic**: Perform addition, subtraction, multiplication, and division operations.
- **Formula Parsing**: Supports mathematical expressions with parentheses and proper operator precedence.
- **Custom Variable Handling**: The app allows for the evaluation of expressions involving variables.
- **Error Handling**: Catches errors such as division by zero and provides meaningful feedback to the user.
  
## Project Structure
- **BasicCalculator.sln**: The main solution file for the project.
- **MainPage.xaml.cs**: Contains the logic and event handling for the calculator's graphical interface.
- **MauiProgram.cs**: Configures the MAUI application, setting up the necessary services and application lifecycle.
- **Formula.cs**: Implements the core logic for parsing and evaluating mathematical expressions, including handling errors and variable substitution.

## Installation
To run the application, ensure you have the following installed:
- [.NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/installation/) 
- .NET SDK 7.0 or later

1. Clone the repository.
   ```bash
   git clone https://github.com/yourusername/BasicCalculator.git
   ```
2. Navigate to the project directory and restore dependencies.
   ```bash
   cd BasicCalculator
   dotnet restore
   ```
3. Build and run the application.
   ```bash
   dotnet build
   dotnet run
   ```

## Usage
- **GUI Interaction**: Use the calculator buttons to input numbers and operators. The result will be displayed after pressing the `=` button.
- **Error Messages**: The app will provide meaningful error messages if invalid operations are performed (e.g., division by zero).

## Contribution
Contributions are welcome! Feel free to open issues or submit pull requests to improve the functionality of this calculator app.

## License
This project is licensed under the MIT License.
