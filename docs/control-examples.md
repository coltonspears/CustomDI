# CustomDI Controls - Usage Examples

This document provides detailed usage examples for the CustomDI.Controls.Wpf library, demonstrating how to use the custom controls in various scenarios.

## Table of Contents
1. [Form with Validation](#form-with-validation)
2. [Data Entry and Display](#data-entry-and-display)
3. [Navigation Interface](#navigation-interface)
4. [Theming and Styling](#theming-and-styling)
5. [Integration with Commands](#integration-with-commands)

## Form with Validation

This example demonstrates how to create a form with validation using CustomTextBox controls.

### XAML
```xml
<StackPanel Margin="20">
    <TextBlock Text="User Registration Form" 
               FontSize="20" 
               FontWeight="Bold" 
               Margin="0,0,0,20"/>
    
    <!-- Username field -->
    <controls:CustomTextBox x:Name="txtUsername"
                           Width="300"
                           HorizontalAlignment="Left"
                           Margin="0,0,0,15"
                           Watermark="Enter username..."
                           Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}"
                           HasValidationError="{Binding HasUsernameError}"
                           ValidationMessage="{Binding UsernameErrorMessage}"/>
    
    <!-- Email field -->
    <controls:CustomTextBox x:Name="txtEmail"
                           Width="300"
                           HorizontalAlignment="Left"
                           Margin="0,0,0,15"
                           Watermark="Enter email address..."
                           Text="{Binding Email, UpdateSourceTrigger=PropertyChanged}"
                           HasValidationError="{Binding HasEmailError}"
                           ValidationMessage="{Binding EmailErrorMessage}"/>
    
    <!-- Password field -->
    <PasswordBox x:Name="txtPassword"
                 Width="300"
                 HorizontalAlignment="Left"
                 Margin="0,0,0,15"
                 PasswordChanged="PasswordBox_PasswordChanged"/>
    
    <!-- Submit button -->
    <controls:CustomButton Content="Register"
                          Width="150"
                          HorizontalAlignment="Left"
                          Command="{Binding RegisterCommand}"
                          Background="#4CAF50"
                          HoverBackground="#388E3C"
                          PressedBackground="#2E7D32"/>
</StackPanel>
```

### ViewModel
```csharp
public class RegistrationViewModel : ViewModelBase
{
    private string _username;
    public string Username
    {
        get => _username;
        set
        {
            SetProperty(ref _username, value, nameof(Username));
            ValidateUsername();
        }
    }

    private string _email;
    public string Email
    {
        get => _email;
        set
        {
            SetProperty(ref _email, value, nameof(Email));
            ValidateEmail();
        }
    }

    private string _password;
    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value, nameof(Password));
            ValidatePassword();
        }
    }

    private bool _hasUsernameError;
    public bool HasUsernameError
    {
        get => _hasUsernameError;
        set => SetProperty(ref _hasUsernameError, value, nameof(HasUsernameError));
    }

    private string _usernameErrorMessage;
    public string UsernameErrorMessage
    {
        get => _usernameErrorMessage;
        set => SetProperty(ref _usernameErrorMessage, value, nameof(UsernameErrorMessage));
    }

    private bool _hasEmailError;
    public bool HasEmailError
    {
        get => _hasEmailError;
        set => SetProperty(ref _hasEmailError, value, nameof(HasEmailError));
    }

    private string _emailErrorMessage;
    public string EmailErrorMessage
    {
        get => _emailErrorMessage;
        set => SetProperty(ref _emailErrorMessage, value, nameof(EmailErrorMessage));
    }

    private bool _hasPasswordError;
    public bool HasPasswordError
    {
        get => _hasPasswordError;
        set => SetProperty(ref _hasPasswordError, value, nameof(HasPasswordError));
    }

    private string _passwordErrorMessage;
    public string PasswordErrorMessage
    {
        get => _passwordErrorMessage;
        set => SetProperty(ref _passwordErrorMessage, value, nameof(PasswordErrorMessage));
    }

    public ICommand RegisterCommand { get; }

    public RegistrationViewModel()
    {
        RegisterCommand = new RelayCommand(Register, CanRegister);
    }

    private void ValidateUsername()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            HasUsernameError = true;
            UsernameErrorMessage = "Username is required";
        }
        else if (Username.Length < 3)
        {
            HasUsernameError = true;
            UsernameErrorMessage = "Username must be at least 3 characters";
        }
        else
        {
            HasUsernameError = false;
            UsernameErrorMessage = string.Empty;
        }
    }

    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            HasEmailError = true;
            EmailErrorMessage = "Email is required";
        }
        else if (!Email.Contains("@") || !Email.Contains("."))
        {
            HasEmailError = true;
            EmailErrorMessage = "Invalid email format";
        }
        else
        {
            HasEmailError = false;
            EmailErrorMessage = string.Empty;
        }
    }

    private void ValidatePassword()
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            HasPasswordError = true;
            PasswordErrorMessage = "Password is required";
        }
        else if (Password.Length < 6)
        {
            HasPasswordError = true;
            PasswordErrorMessage = "Password must be at least 6 characters";
        }
        else
        {
            HasPasswordError = false;
            PasswordErrorMessage = string.Empty;
        }
    }

    private bool CanRegister()
    {
        return !HasUsernameError && !HasEmailError && !HasPasswordError &&
               !string.IsNullOrWhiteSpace(Username) &&
               !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(Password);
    }

    private void Register()
    {
        // Registration logic here
        MessageBox.Show($"User {Username} registered successfully!", "Registration", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
```

### Code-behind
```csharp
public partial class RegistrationView : UserControl
{
    public RegistrationView()
    {
        InitializeComponent();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegistrationViewModel viewModel)
        {
            viewModel.Password = ((PasswordBox)sender).Password;
        }
    }
}
```

## Data Entry and Display

This example demonstrates how to use CustomComboBox and CustomDataGrid for data entry and display.

### XAML
```xml
<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <TextBlock Grid.Row="0" 
               Text="Product Management" 
               FontSize="20" 
               FontWeight="Bold" 
               Margin="0,0,0,20"/>

    <!-- Data Entry Form -->
    <Grid Grid.Row="1" Margin="0,0,0,20">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="Auto"/>
        </Grid.ColumnDefinitions>

        <!-- Product Name -->
        <controls:CustomTextBox Grid.Column="0"
                               Margin="0,0,10,0"
                               Watermark="Product name..."
                               Text="{Binding NewProduct.Name, UpdateSourceTrigger=PropertyChanged}"/>

        <!-- Product Category -->
        <controls:CustomComboBox Grid.Column="1"
                                Margin="10,0,10,0"
                                Watermark="Select category..."
                                ItemsSource="{Binding Categories}"
                                DisplayMemberPath="Name"
                                SelectedItem="{Binding SelectedCategory}"/>

        <!-- Add Button -->
        <controls:CustomButton Grid.Column="2"
                              Content="Add Product"
                              Command="{Binding AddProductCommand}"
                              Background="#2196F3"
                              HoverBackground="#1976D2"
                              PressedBackground="#0D47A1}"/>
    </Grid>

    <!-- Products DataGrid -->
    <controls:CustomDataGrid Grid.Row="2"
                            ItemsSource="{Binding Products}"
                            AutoGenerateColumns="False"
                            CornerRadius="5"
                            HeaderBackground="#E3F2FD"
                            AlternateRowBackground="#F5F5F5"
                            SelectedRowBackground="#BBDEFB"
                            GridLinesBrush="#E0E0E0"
                            EmptyMessage="No products available"
                            ShowEmptyMessage="True">
        <controls:CustomDataGrid.Columns>
            <DataGridTextColumn Header="ID" Binding="{Binding Id}" Width="50"/>
            <DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="*"/>
            <DataGridTextColumn Header="Category" Binding="{Binding Category.Name}" Width="150"/>
            <DataGridTextColumn Header="Price" Binding="{Binding Price, StringFormat=C}" Width="100"/>
            <DataGridCheckBoxColumn Header="In Stock" Binding="{Binding InStock}" Width="80"/>
            <DataGridTemplateColumn Header="Actions" Width="100">
                <DataGridTemplateColumn.CellTemplate>
                    <DataTemplate>
                        <StackPanel Orientation="Horizontal">
                            <Button Content="Edit" Margin="0,0,5,0" 
                                    Command="{Binding DataContext.EditProductCommand, 
                                    RelativeSource={RelativeSource AncestorType=controls:CustomDataGrid}}"
                                    CommandParameter="{Binding}"/>
                            <Button Content="Delete" 
                                    Command="{Binding DataContext.DeleteProductCommand, 
                                    RelativeSource={RelativeSource AncestorType=controls:CustomDataGrid}}"
                                    CommandParameter="{Binding}"/>
                        </StackPanel>
                    </DataTemplate>
                </DataGridTemplateColumn.CellTemplate>
            </DataGridTemplateColumn>
        </controls:CustomDataGrid.Columns>
    </controls:CustomDataGrid>
</Grid>
```

### ViewModel
```csharp
public class ProductManagementViewModel : ViewModelBase
{
    private ObservableCollection<Product> _products;
    public ObservableCollection<Product> Products
    {
        get => _products;
        set => SetProperty(ref _products, value, nameof(Products));
    }

    private ObservableCollection<Category> _categories;
    public ObservableCollection<Category> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value, nameof(Categories));
    }

    private Category _selectedCategory;
    public Category SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value, nameof(SelectedCategory));
    }

    private Product _newProduct;
    public Product NewProduct
    {
        get => _newProduct;
        set => SetProperty(ref _newProduct, value, nameof(NewProduct));
    }

    public ICommand AddProductCommand { get; }
    public ICommand EditProductCommand { get; }
    public ICommand DeleteProductCommand { get; }

    public ProductManagementViewModel()
    {
        // Initialize collections
        Categories = new ObservableCollection<Category>
        {
            new Category { Id = 1, Name = "Electronics" },
            new Category { Id = 2, Name = "Clothing" },
            new Category { Id = 3, Name = "Books" },
            new Category { Id = 4, Name = "Home & Kitchen" }
        };

        Products = new ObservableCollection<Product>
        {
            new Product { Id = 1, Name = "Laptop", Category = Categories[0], Price = 999.99m, InStock = true },
            new Product { Id = 2, Name = "T-Shirt", Category = Categories[1], Price = 19.99m, InStock = true },
            new Product { Id = 3, Name = "Novel", Category = Categories[2], Price = 9.99m, InStock = false }
        };

        // Initialize new product
        NewProduct = new Product();

        // Initialize commands
        AddProductCommand = new RelayCommand(AddProduct, CanAddProduct);
        EditProductCommand = new RelayCommand<Product>(EditProduct);
        DeleteProductCommand = new RelayCommand<Product>(DeleteProduct);
    }

    private bool CanAddProduct()
    {
        return !string.IsNullOrWhiteSpace(NewProduct.Name) && SelectedCategory != null;
    }

    private void AddProduct()
    {
        var product = new Product
        {
            Id = Products.Count > 0 ? Products.Max(p => p.Id) + 1 : 1,
            Name = NewProduct.Name,
            Category = SelectedCategory,
            Price = NewProduct.Price,
            InStock = NewProduct.InStock
        };

        Products.Add(product);
        NewProduct = new Product();
        SelectedCategory = null;
    }

    private void EditProduct(Product product)
    {
        // Implement edit logic
    }

    private void DeleteProduct(Product product)
    {
        Products.Remove(product);
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Category Category { get; set; }
    public decimal Price { get; set; }
    public bool InStock { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

## Navigation Interface

This example demonstrates how to create a navigation interface using CustomButton controls.

### XAML
```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- Navigation Bar -->
    <Border Grid.Row="0" 
            Background="#2196F3" 
            Padding="10">
        <StackPanel Orientation="Horizontal">
            <controls:CustomButton Content="Dashboard"
                                  Margin="0,0,5,0"
                                  Background="Transparent"
                                  BorderThickness="0"
                                  Foreground="White"
                                  HoverBackground="#1976D2"
                                  PressedBackground="#0D47A1"
                                  Command="{Binding NavigateCommand}"
                                  CommandParameter="Dashboard"/>
            
            <controls:CustomButton Content="Products"
                                  Margin="5,0,5,0"
                                  Background="Transparent"
                                  BorderThickness="0"
                                  Foreground="White"
                                  HoverBackground="#1976D2"
                                  PressedBackground="#0D47A1"
                                  Command="{Binding NavigateCommand}"
                                  CommandParameter="Products"/>
            
            <controls:CustomButton Content="Customers"
                                  Margin="5,0,5,0"
                                  Background="Transparent"
                                  BorderThickness="0"
                     <response clipped><NOTE>To save on context only part of this file has been shown to you. You should retry this tool after you have searched inside the file with `grep -n` in order to find the line numbers of what you are looking for.</NOTE>