using SpreadsheetUtilities;


namespace BasicCalculator;
public partial class MainPage : ContentPage
{
    private string ANS = "0";
    public MainPage()
    {
        InitializeComponent();
        this.SizeChanged += OnPageSizeChanged;

    }
    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        var newSize = this.Bounds.Size;

        Console.WriteLine($"New Size: Width={newSize.Width}, Height={newSize.Height}");
    }

    private void NumberClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        entry.Text = entry.Text + button.Text;
    }

    private void FunctionClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        switch (button.ClassId)
        {
            case "buttonDel":
                entry.Text = entry.Text.Substring(0, entry.Text.Length - 1);
                break;
            case "buttonAC":
                entry.Text = "";
                break;
            case "buttonEqual":
                Evaluate();
                break;
        }
    }

    private void Evaluate()
    {

        string answer = "";
        try
        {
            Formula formula = new Formula(replaceVar(entry.Text));
            answer = formula.Evaluate(s => double.Parse(s)).ToString();
            ANS = answer;
            if (answer.Contains("Spreads"))
            {
                string errorMessage = answer.Substring(answer.LastIndexOf('.') + 1);
                answer = "";
                throw new Exception(errorMessage);
            }
        }
        catch (Exception e)
        {
            this.DisplayAlert("ERROR", e.Message, "try again");
        }
        entry.Text = answer;
    }

    private string replaceVar(string formula)
    {
        formula = formula.Replace("x", "*");
        formula = formula.Replace("ANS", ANS);
        return formula;
    }

    private List<Button> GetAllButtons(View view)
    {
        var buttons = new List<Button>();

        if (view is Button button)
        {
            buttons.Add(button);
        }
        else if (view is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                buttons.AddRange(GetAllButtons((View)child));
            }
        }
        else if (view is ContentView contentView)
        {
            buttons.AddRange(GetAllButtons(contentView.Content));
        }

        return buttons;
    }
}


