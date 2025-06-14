using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NotesApi.Models;
using NotesApi.Services.IService;
using Xunit;

namespace NotesApi.Tests
{
    public class NoteServiceTests
    {
        private readonly Mock<INoteService> _noteServiceMock;
        private const int TestUserId = 1;

        public NoteServiceTests()
        {
            _noteServiceMock = new Mock<INoteService>();
        }

        [Fact]
        public async Task GetAllNotesByUserId_ReturnsListOfNotes()
        {
            // Arrange
            var notes = new List<Note>
            {
                new Note { Id = 1, UserId = TestUserId, Title = "Test Note 1", Content = "Content 1" },
                new Note { Id = 2, UserId = TestUserId, Title = "Test Note 2", Content = "Content 2" }
            };

            _noteServiceMock.Setup(service => service.GetAllNotesByUserIdAsync(TestUserId))
                .ReturnsAsync(notes);

            // Act
            var result = await _noteServiceMock.Object.GetAllNotesByUserIdAsync(TestUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, note => Assert.Equal(TestUserId, note.UserId));
        }

        [Fact]
        public async Task GetNoteById_ExistingId_ReturnsNote()
        {
            // Arrange
            var note = new Note { Id = 1, UserId = TestUserId, Title = "Test Note", Content = "Test Content" };

            _noteServiceMock.Setup(service => service.GetNoteByIdAsync(1))
                .ReturnsAsync(note);

            // Act
            var result = await _noteServiceMock.Object.GetNoteByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Note", result.Title);
        }

        [Fact]
        public async Task CreateNote_ValidNote_CreatesNote()
        {
            // Arrange
            var newNote = new Note { UserId = TestUserId, Title = "New Note", Content = "New Content" };

            _noteServiceMock.Setup(service => service.CreateNoteAsync(newNote))
                .Returns(Task.CompletedTask);

            // Act
            await _noteServiceMock.Object.CreateNoteAsync(newNote);

            // Assert
            _noteServiceMock.Verify(service => service.CreateNoteAsync(newNote), Times.Once);
        }

        [Fact]
        public async Task UpdateNote_ExistingNote_ReturnsTrue()
        {
            // Arrange
            var existingNote = new Note { Id = 1, UserId = TestUserId, Title = "Updated Note", Content = "Updated Content" };

            _noteServiceMock.Setup(service => service.UpdateNoteAsync(TestUserId, existingNote))
                .ReturnsAsync(true);

            // Act
            var result = await _noteServiceMock.Object.UpdateNoteAsync(TestUserId, existingNote);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteNote_ExistingId_ReturnsTrue()
        {
            // Arrange
            int noteId = 1;

            _noteServiceMock.Setup(service => service.DeleteNoteAsync(noteId, TestUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _noteServiceMock.Object.DeleteNoteAsync(noteId, TestUserId);

            // Assert
            Assert.True(result);
        }
    }
}
