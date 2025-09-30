using ksiazka2.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Contact = ksiazka2.Models.Contact;

namespace ksiazka2
{
    public partial class MainPage : ContentPage
    {
        private ContactsRepository _repository;
        private Contact _contact;

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshListView();
        }

        public MainPage()
        {
            InitializeComponent();

            _repository = new ContactsRepository();
            _contact = new Contact();
            _repository.Initialize();
            if (_repository.Contacts.Count == 0)
                infoText.Text = "Brak kontaktów w bazie";
            else
                infoText.Text = "";
            contactsListView.ItemsSource = _repository.Contacts;
            BindingContext = _contact;
            PagLabel.Text = _repository.Offset.ToString();
        }


      
        private async void OnEditClicked(object sender, EventArgs e)
        {
            Contact contact = null;

            if (sender is MenuFlyoutItem menuItem)
                contact = menuItem.CommandParameter as Contact;
            else if (sender is Button button)
                contact = button.CommandParameter as Contact;

            if (contact != null)
            {
                await Navigation.PushAsync(new Pages.EditContact(contact, _repository));
            }
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            Contact contact = null;

            if (sender is MenuFlyoutItem menuItem)
                contact = menuItem.CommandParameter as Contact;
            else if (sender is Button button)
                contact = button.CommandParameter as Contact;

            if (contact != null)
            {
                _repository.RemoveContact(contact);
                RefreshListView();
            }
        }
        private async void OnAddContactClicked(object sender, EventArgs e)
        {
           await Navigation.PushAsync(new Pages.AddContact(_repository));
        }

        private void RefreshListView()
        {
            
         

            if (!string.IsNullOrWhiteSpace(searchbar.Text))
            {
                _repository.LoadContactsPage(true);
                string term = searchbar.Text.ToLower();
                var results = _repository.Contacts.Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    c.LastName.ToLower().Contains(term) ||
                    c.PhoneNumber.ToLower().Contains(term)
                ).ToList();
                contactsListView.ItemsSource = results.OrderBy(c => c.LastName).ThenBy(c => c.Name).ToList();
                PagLabel.Text = "1";
            }
            else
            {
                _repository.LoadContactsPage();
                contactsListView.ItemsSource = _repository.Contacts
             .OrderBy(c => c.LastName).ThenBy(c => c.Name).ToList();
                PagLabel.Text = _repository.Offset.ToString();
            }
          
        }

        private void PagLeftClicked(object sender, EventArgs e)
        {
            _repository.PageLeft();
            RefreshListView();
        }

        private void PagRightClicked(object sender, EventArgs e)
        {
            _repository.PageRight();
            RefreshListView();
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshListView();
        }
    }
}
