using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sudoku.WinForms;

namespace Sudoku.Tests
{
    [TestClass]
    public sealed class SudokuFormIntegrationTests
    {
        [TestMethod]
        public void MainForm_RequiredButtonsArePresentAndWired()
        {
            Exception failure = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using (var form = new SudokuForm())
                    {
                        form.Show();
                        Application.DoEvents();

                        var buttons = Descendants(form).OfType<Button>().ToDictionary(button => button.Text);
                        foreach (var required in new[] { "Hint", "Check", "Clear Cell", "New Game", "Quit" })
                            Assert.IsTrue(buttons.ContainsKey(required), $"The {required} button is missing.");

                        var status = Descendants(form).OfType<Label>()
                            .Single(label => label.AccessibleName == "Game status");
                        buttons["Hint"].PerformClick();
                        StringAssert.Contains(status.Text, "Hint:");
                        buttons["Check"].PerformClick();
                        Assert.AreEqual("No rule violations detected.", status.Text);
                        buttons["Clear Cell"].PerformClick();
                        StringAssert.Contains(status.Text, "Select an editable cell");
                        buttons["New Game"].PerformClick();
                        StringAssert.Contains(status.Text, "New 30-clue puzzle created");
                        buttons["Quit"].PerformClick();
                        Assert.IsFalse(form.Visible);
                    }
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (failure != null) throw new AssertFailedException("The WinForms integration test failed.", failure);
        }

        private static IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var descendant in Descendants(child)) yield return descendant;
            }
        }
    }
}
