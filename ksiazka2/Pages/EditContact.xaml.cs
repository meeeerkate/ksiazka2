using ksiazka2.Models;
namespace ksiazka2.Pages;
using Contact = ksiazka2.Models.Contact;
public partial class EditContact : ContentPage
{
    private readonly Contact _oldContact;
    private readonly ContactsRepository _repository;

    public EditContact(Contact contact, ContactsRepository repository)
    {
        InitializeComponent();

        // Kopia oryginalnego kontaktu (wa¿ny Id!)
        _oldContact = contact;
        _repository = repository;

        // Ustawienie pól
        NameEntry.Text = contact.Name;
        LastNameEntry.Text = contact.LastName;
        PhoneEntry.Text = contact.PhoneNumber;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(LastNameEntry.Text) ||
            string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            await DisplayAlert("B³¹d", "Uzupe³nij wszystkie pola.", "OK");
            return;
        }

        // Nowa wersja kontaktu (bez zmiany Id)
        var newContact = new Contact
        {
            Id = _oldContact.Id,
            Name = NameEntry.Text,
            LastName = LastNameEntry.Text,
            PhoneNumber = PhoneEntry.Text
        };

        _repository.EditContact(_oldContact, newContact);

        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}