using ksiazka2.Models;
using Contact = ksiazka2.Models.Contact;

namespace ksiazka2.Pages;

public partial class AddContact : ContentPage
{
    private readonly ContactsRepository _repository;

    public AddContact(ContactsRepository repository)
    {
        InitializeComponent();
        _repository = repository;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(LastNameEntry.Text) ||
            string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            await DisplayAlert("B³¹d", "Wype³nij wszystkie pola.", "OK");
            return;
        }

        var newContact = new Contact
        {
            Name = NameEntry.Text,
            LastName = LastNameEntry.Text,
            PhoneNumber = PhoneEntry.Text
        };

        _repository.AddContact(newContact);

        await Navigation.PopAsync(); // wróæ do MainPage
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
