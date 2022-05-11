namespace COMETwebapp.Model
{
    public class ApplicationCards
    {
        /// <summary>
        /// List of application cards with Name, Color, Icon and Description data
        /// </summary>
        public List<Card> Cards { get; set; } = new List<Card>()
        {
            new Card()
            {
                Name = "Parameter Editor",
                Color = "#76b8fc",
                Icon = "spreadsheet",
                Description = "Table of element usages with their associated parameters."
            },
            new Card()
            {
                Name = "Model Dashboard",
                Color = "#c3cffd",
                Icon = "task",
                Description = "Summarize the model progress."
            },
            new Card()
            {
                Name = "Subscription Dashboard",
                Color = "#76fd98",
                Icon = "person",
                Description = "Table of subscribed values."
            },
            new Card()
            {
                Name = "System Representation",
                Color = "#c3fe9f",
                Icon = "fork",
                Description = "Represent relations between elements."
            },
            new Card()
            {
                Name = "Report Preview",
                Color = "#ecffa1",
                Icon = "book",
                Description = "Preview of the actual report state."
            },
            new Card()
            {
                Name = "Requirement Management",
                Color = "#fda966",
                Icon = "link-intact",
                Description = "Edit requirements in the model."
            },
            new Card()
            {
                Name = "Budget Editor",
                Color = "#ffe86a",
                Icon = "brush",
                Description = "Create budget tables."
            }
        };
    }
}
