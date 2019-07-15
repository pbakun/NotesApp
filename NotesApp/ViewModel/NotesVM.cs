using NotesApp.Model;
using NotesApp.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NotesApp.ViewModel
{
    public class NotesVM : INotifyPropertyChanged
    {
        private bool isEditing;

        public bool IsEditing
        {
            get { return isEditing; }
            set
            {
                isEditing = value;
                OnPropertyChange(nameof(IsEditing));
            }
        }

        private bool isEditingNote;

        public bool IsEditingNote
        {
            get { return isEditingNote; }
            set { isEditingNote = value;
                OnPropertyChange(nameof(IsEditingNote));
            }
        }


        public ObservableCollection<Notebook> Notebooks { get; set; }

        private Notebook selectedNotebook;

        public Notebook SelectedNotebook
        {
            get { return selectedNotebook; }
            set
            {
                selectedNotebook = value;
                ReadNotes();
            }
        }

        private Note selectedNote;

        public Note SelectedNote
        {
            get { return selectedNote; }
            set
            {
                selectedNote = value;
                SelectedNoteChanged(this, new EventArgs());
            }
        }


        public ObservableCollection<Note> Notes { get; set; }

        public NewNoteCommand NewNoteCommand { get; set; }
        public NewNotebookCommand NewNotebookCommand { get; set; }
        public BeginEditCommand BeginEditCommand { get; set; }
        public HasEditedCommand HasEditedCommand { get; set; }
        public DeleteCommand DeleteCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler SelectedNoteChanged;

        public NotesVM()
        {
            IsEditing = false;
            IsEditingNote = false;

            NewNotebookCommand = new NewNotebookCommand(this);
            NewNoteCommand = new NewNoteCommand(this);
            BeginEditCommand = new BeginEditCommand(this);
            HasEditedCommand = new HasEditedCommand(this);
            DeleteCommand = new DeleteCommand(this);

            Notebooks = new ObservableCollection<Notebook>();
            Notes = new ObservableCollection<Note>();

            ReadNotebooks();
            ReadNotes();
        }

        private void OnPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void CreateNote(int notebookId)
        {
            Note newNote = new Note()
            {
                NotebookId = notebookId,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                Title = "New note"
            };

            DatabaseHelper.Insert(newNote);

            ReadNotebooks();
            ReadNotes();
        }

        public void CreateNotebook()
        {
            Notebook notebook = new Notebook()
            {
                Name = "new Notebook",
                UserId = int.Parse(App.UserId)
            };

            DatabaseHelper.Insert(notebook);

            ReadNotebooks();

        }

        public void ReadNotebooks()
        {
                using (SQLite.SQLiteConnection connection = new SQLite.SQLiteConnection(DatabaseHelper.dbFile))
                {
                    connection.CreateTable<Notebook>();
                    var notebooks = connection.Table<Notebook>().ToList();

                    Notebooks.Clear();
                    foreach (var notebook in notebooks)
                    {
                        Notebooks.Add(notebook);
                    }
                }
        }

        public void ReadNotes()
        {
            try
            {
                using (SQLite.SQLiteConnection connection = new SQLite.SQLiteConnection(DatabaseHelper.dbFile))
                {
                    if (SelectedNotebook != null)
                    {
                        var notes = connection.Table<Note>().Where(n => n.NotebookId == SelectedNotebook.Id).ToList();

                        Notes.Clear();
                        foreach (var note in notes)
                        {
                            Notes.Add(note);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public void StartEditing(object parameter)
        {
            var type = parameter.GetType();
            if(type == typeof(Notebook))
                IsEditing = true;

            if (type == typeof(Note))
                IsEditingNote = true;
        }

        public void HasRenamed(object parameter)
        {
            var type = parameter.GetType();
            if (parameter != null)
            {
                if (type == typeof(Notebook))
                {
                    var notebook = parameter as Notebook;
                    DatabaseHelper.Update(notebook);
                    IsEditing = false;
                    ReadNotebooks();
                }
                else if(type == typeof(Note))
                {
                    var note = parameter as Note;
                    DatabaseHelper.Update(note);
                    IsEditingNote = false;
                    ReadNotes();
                }
                
            }
        }

        public void UpdateSelectedNote()
        {
            DatabaseHelper.Update(SelectedNote);
        }

        public void Delete(object parameter)
        {
            if(parameter != null)
            {
                var type = parameter.GetType();
                if(type == typeof(Notebook))
                {
                    var notebook = parameter as Notebook;
                    DatabaseHelper.Delete(notebook);
                }
                if(type == typeof(Note))
                {
                    var note = parameter as Note;
                    DatabaseHelper.Delete(note);
                }
                ReadNotebooks();
                ReadNotes();
            }
        }
    }
}
