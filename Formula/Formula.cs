// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!
// Last updated: August 2023 (small tweak to API)
/**
 * University of Utah Course: CS 3500-001 Fall 2023 Software Practice
 *
 * Semester: Fall 2023
 *
 * Assignment:2
 *
 * @Author: Duy Khanh Tran
 *
 * Created date: 09/14/2023
 *
 * Description: Creating a Formula class which is a more generalized version of your FormulaEvaluator work.
 */
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities;

/// <summary>
/// Represents formulas written in standard infix notation using standard precedence
/// rules.  The allowed symbols are non-negative numbers written using double-precision
/// floating-point syntax (without unary preceeding '-' or '+');
/// variables that consist of a letter or underscore followed by
/// zero or more letters, underscores, or digits; parentheses; and the four operator
/// symbols +, -, *, and /.
///
/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
/// and "x 23" consists of a variable "x" and a number "23".
///
/// Associated with every formula are two delegates: a normalizer and a validator.  The
/// normalizer is used to convert variables into a canonical form. The validator is used to
/// add extra restrictions on the validity of a variable, beyond the base condition that
/// variables must always be legal: they must consist of a letter or underscore followed
/// by zero or more letters, underscores, or digits.
/// Their use is described in detail in the constructor and method comments.
/// </summary>
public class Formula
{
    private string formula;
    private Stack<double> numStack;
    private Stack<string> operatorStack;
    private bool errorCall;
    private string errorType;
    // private bool lastParenthasisExist;
    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically invalid,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer is the identity function, and the associated validator
    /// maps every string to true.
    /// </summary>
    public Formula(string formula) :
        this(formula, s => s, s => true)
    {
        //leave this empty
    }


    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically incorrect,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer and validator are the second and third parameters,
    /// respectively.
    ///
    /// If the formula contains a variable v such that normalize(v) is not a legal variable,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// If the formula contains a variable v such that isValid(normalize(v)) is false,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// Suppose that N is a method that converts all the letters in a string to upper case, and
    /// that V is a method that returns true only if a string consists of one letter followed
    /// by one digit.  Then:
    ///
    /// new Formula("x2+y3", N, V) should succeed
    /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
    /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
    /// </summary>
    public Formula(string formula, Func<string, string> normalize, Func<string, bool> isValid)
    {
        //assign error type if exist
        errorType = "";
        //assign and remove all the space
        this.formula = formula;
        //set up number and stack variables
        numStack = new Stack<double>();
        operatorStack = new Stack<string>();
        //check and reassign the formula after validate and normalize
        foreach (string tokenVariable in GetTokens(this.formula))
        {
            //make operators list to check it later
            string[] operations = new string[] { "+", "-", "*", "/", "(", ")" };
            //make a temporary token variable
            string tempToken = "";
            //check if it's a variable
            if (isVariable(tokenVariable))
            {
                //normalize it
                try
                {
                    //assign the normalized version to the temporary token
                    tempToken = normalize(tokenVariable);
                }
                catch (Exception)
                {
                    throw new FormulaFormatException("error during normalization process");
                }
                //validate the token
                if (!isValid(tempToken))
                    throw new FormulaFormatException("the formula after normalization is invalid");
                //if it's valid, replace the old variable with the validated one
                else this.formula = this.formula.Replace(tokenVariable, tempToken);
            }
            else
            {
                if (!double.TryParse(tokenVariable, out _) && !operations.Contains(tokenVariable))
                    throw new FormulaFormatException("there exist an invalid operation");
            }
        }
        haveSyntaxError(GetTokens(this.formula));
    }

    /// <summary>
    /// Evaluates this Formula, using the lookup delegate to determine the values of
    /// variables.  When a variable symbol v needs to be determined, it should be looked up
    /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to
    /// the constructor.)
    ///
    /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters
    /// in a string to upper case:
    ///
    /// new Formula("x+7", N, s => true).Evaluate(L) is 11
    /// new Formula("x+7").Evaluate(L) is 9
    ///
    /// Given a variable symbol as its parameter, lookup returns the variable's value
    /// (if it has one) or throws an ArgumentException (otherwise).
    ///
    /// If no undefined variables or divisions by zero are encountered when evaluating
    /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.
    /// The Reason property of the FormulaError should have a meaningful explanation.
    ///
    /// This method should never throw an exception.
    /// </summary>
    public object Evaluate(Func<string, double> lookup)
    {
        //put each character into a list
        string[] substrings = Regex.Split(formula, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
        for (int i = 0; i < substrings.Length; i++)
        {
            substrings[i] = substrings[i].Trim();
            //check if it's an empty String or white space or null
            if (string.IsNullOrEmpty(substrings[i]) || string.IsNullOrWhiteSpace(substrings[i]))
                continue;
            double num;
            //check for operation
            string[] operations = new string[] { "+", "-", "*", "/", "(", ")" };
            if (operations.Contains(substrings[i]))
            {
                operationIterate(substrings[i]);
                if (errorCall)
                    return new FormulaError(errorType);
            }
            //check for number
            else if (double.TryParse(substrings[i], out num))
            {
                numIterate(num);
                if (errorCall)
                    return new FormulaError(errorType);
            }
            //check variables
            else if (isVariable(substrings[i]))
            {
                double lookUpVari;
                try
                {
                    lookUpVari = lookup(substrings[i]);
                }
                catch (Exception)
                {
                    return new FormulaError("no such variable exist");
                }
                numIterate(lookUpVari);
                if (errorCall)
                    return new FormulaError(errorType);
            }
            else return new FormulaError("invalid expression");
        }
        //after the last token been processeed
        if (numStack.Count == 0)
            return new FormulaError("given equation is empty");
        if (operatorStack.Count() == 0)
            return numStack.Pop();
        else
        {
            pushDoublePop();
            return numStack.Pop();
        }
    }

    /// <summary>
    /// Enumerates the normalized versions of all of the variables that occur in this
    /// formula.  No normalization may appear more than once in the enumeration, even
    /// if it appears more than once in this Formula.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
    /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
    /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
    /// </summary>
    public IEnumerable<string> GetVariables()
    {
        List<string> variables = new List<string>();
        //add in elements into the list
        foreach (string expression in GetTokens(formula))
            if (isVariable(expression))
                variables.Add(expression);
        //remove duplicates
        return variables.Distinct();
    }

    /// <summary>
    /// Returns a string containing no spaces which, if passed to the Formula
    /// constructor, will produce a Formula f such that this.Equals(f).  All of the
    /// variables in the string should be normalized.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
    /// new Formula("x + Y").ToString() should return "x+Y"
    /// </summary>
    public override string ToString()
    {
        return formula.Replace(" ", "");
    }

    /// <summary>
    /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
    /// whether or not this Formula and obj are equal.
    ///
    /// Two Formulae are considered equal if they consist of the same tokens in the
    /// same order.  To determine token equality, all tokens are compared as strings
    /// except for numeric tokens and variable tokens.
    /// Numeric tokens are considered equal if they are equal after being "normalized" by
    /// using C#'s standard conversion from string to double (and optionally back to a string).
    /// Variable tokens are considered equal if their normalized forms are equal, as
    /// defined by the provided normalizer.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
    /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
    /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
    /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is Formula))
            return false;
        //cast
        Formula other = (Formula)obj;
        IEnumerable<string> otherTokens = GetTokens(other.ToString());
        IEnumerable<string> thisTokens = GetTokens(ToString());
        if (otherTokens.Count() != thisTokens.Count())
            return false;
        else
        {
            IEnumerator<string> otherEnumerator = otherTokens.GetEnumerator();
            IEnumerator<string> thisEnumerator = thisTokens.GetEnumerator();
            while (otherEnumerator.MoveNext() && thisEnumerator.MoveNext())
            {
                //check if it's a number, if it is (compare them)
                double otherValue;
                double thisValue;
                if (double.TryParse(otherEnumerator.Current, out otherValue) && double.TryParse(thisEnumerator.Current, out thisValue))
                {
                    if (otherValue != thisValue)
                        return false;
                }
                else if (!otherEnumerator.Current.Equals(thisEnumerator.Current))
                    return false;
            }
            return true;
        }
    }
    /// <summary>
    /// Reports whether f1 == f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator ==(Formula f1, Formula f2)
    {
        if (f1 is null || f2 is null)
            return false;
        return f1.Equals(f2);
    }

    /// <summary>
    /// Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator !=(Formula f1, Formula f2)
    {
        if (f1 is null || f2 is null)
            return false;
        return !f1.Equals(f2);
    }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    /// randomly-generated unequal Formulae have the same hash code should be extremely small.
    /// </summary>
    public override int GetHashCode()
    {
        int returnHash = 17; //pick a popular prime number
        IEnumerable<string> tokens = GetTokens(ToString());
        foreach (string token in tokens)
        {
            double parseValue;
            if (double.TryParse(token, out parseValue))
                returnHash = returnHash * 23 + parseValue.GetHashCode();
            else returnHash = returnHash * 23 + token.GetHashCode();
        }
        return returnHash;
    }

    /// <summary>
    /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
    /// right paren; one of the four operator symbols; a legal variable token;
    /// a double literal; and anything that doesn't match one of those patterns.
    /// There are no empty tokens, and no token contains white space.
    /// </summary>
    private static IEnumerable<string> GetTokens(string formula)
    {
        // Patterns for individual tokens
        string lpPattern = @"\(";
        string rpPattern = @"\)";
        string opPattern = @"[\+\-*/]";
        string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                yield return s;
            }
        }

    }
    //-------------------------------MY OWN PRIVATE METHODS-------------------------------//

    /// <summary>
    /// Checks the syntax of a list of expressions for a formula.
    /// </summary>
    /// <param name="list">The list of expressions to check.</param>
    /// <exception cref="FormulaFormatException">
    ///   <para>Thrown when:</para>
    ///   <list type="bullet">
    ///     <item>The list is empty.</item>
    ///     <item>The first expression is invalid (not a number, variable, or opening parenthesis).</item>
    ///     <item>The last expression is invalid (not a number, variable, or closing parenthesis).</item>
    ///     <item>A follow-up expression is invalid based on the preceding expression and operators.</item>
    ///     <item>There are extra closing parentheses.</item>
    ///     <item>There are extra opening parentheses.</item>
    ///   </list>
    /// </exception>
    private void haveSyntaxError(IEnumerable<string> list)
    {
        //Check if it's empty
        if (string.IsNullOrEmpty(this.formula) || string.IsNullOrWhiteSpace(this.formula))
            throw new FormulaFormatException("the formula is empty");
        //need right perenthesis count to compart close parenthesis
        int closeParenCount = 0;
        int openParenCount = 0;
        // have a position count to keep track of crucial position
        int listPos = 0;
        //keep track of list of operator and have a previous espression check
        string previousExpression = "";
        string[] operatorsList = new string[] { "+", "-", "*", "/" };
        foreach (string expression in list)
        {
            listPos++;
            // if it's the first expression
            if (listPos == 1)
            {
                //also assign the first expression
                previousExpression = expression;
                if (!double.TryParse(expression, out _) && !isVariable(expression) && !expression.Equals("("))
                    throw new FormulaFormatException("your first expression is invalid");
            }
            //if it's the last expression
            else if (listPos == list.Count())
            {
                if (!double.TryParse(expression, out _) && !isVariable(expression) && !expression.Equals(")"))
                    throw new FormulaFormatException("your last expression is invalid");

                //check token that follows "(", "+", "-", "*", "/"
                else if (operatorsList.Contains(previousExpression) || previousExpression.Equals("("))
                {
                    if (!double.TryParse(expression, out _) && !isVariable(expression) && !expression.Equals("("))
                        throw new FormulaFormatException("your follow-up is invalid");
                }
                //check token that follows  number, a variable, or a closing parenthesis
                else if (double.TryParse(previousExpression, out _) || isVariable(previousExpression) || previousExpression.Equals(")"))
                {
                    if (!operatorsList.Contains(expression) && !expression.Equals(")")) //might be wrong, it was ! before
                        throw new FormulaFormatException("your follow-up is invalid");
                }

            }
            //if it's other position
            else
            {
                //check token that follows "(", "+", "-", "*", "/"
                if (operatorsList.Contains(previousExpression) || previousExpression.Equals("("))
                {
                    if (!double.TryParse(expression, out _) && !isVariable(expression) && !expression.Equals("("))
                        throw new FormulaFormatException("your follow-up is invalid");
                }
                //check token that follows  number, a variable, or a closing parenthesis
                else if (double.TryParse(previousExpression, out _) || isVariable(previousExpression) || previousExpression.Equals(")"))
                {
                    if (!operatorsList.Contains(expression) && !expression.Equals(")"))
                        throw new FormulaFormatException("your follow-up is invalid");
                }
                //also assign the expression
                previousExpression = expression;
            }
            //count the open and the close parenthesis
            switch (expression)
            {
                case "(":
                    openParenCount++;
                    break;
                case ")":
                    closeParenCount++;
                    break;
            }
            //check parenthesis
            if (closeParenCount > openParenCount)
                throw new FormulaFormatException("you are having extra close parentheses");
        }
        //comparte parenthesis count
        if (closeParenCount != openParenCount)
            throw new FormulaFormatException("you are having extra parentheses");

    }

    /// <summary>
    /// determines whether the given expression is a variable name.
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    /// <returns>
    ///  true if the expression is a valid variable name; otherwise, false
    /// </returns>
    private bool isVariable(string expression)
    {
        return Regex.IsMatch(expression, @"^[a-zA-Z_]+[a-zA-Z_0-9]*$");
    }

    /// <summary>
    /// Processes a numeric operand during expression evaluation
    /// </summary>
    /// <param name="currentNum"></param>
    private void numIterate(double currentNum)
    {
        //if "*" or "/" is on top
        if ((numStack.Count > 0) && (operatorStack.Count > 0) && operatorStack.Peek().Equals("*") || (numStack.Count > 0) && (operatorStack.Count > 0) && operatorStack.Peek().Equals("/"))
            numStack.Push(evaluate(operatorStack.Pop(), currentNum, numStack.Pop()));
        else
            numStack.Push(currentNum);
    }
    /// <summary>
    /// processes an operator during expression evaluation
    /// </summary>
    /// <param name="operation">the operator to processparam>
    private void operationIterate(String operation)
    {
        //if it's "+" "-" 
        string[] shouldDoMoreAction = new string[] { "+", "-" };
        if (shouldDoMoreAction.Contains(operation))
            //if it's "+" or "- on top
            if ((numStack.Count > 1 && operatorStack.Peek().Equals("+")) || (numStack.Count > 1 && operatorStack.Peek().Equals("-")))
            {
                pushDoublePop();
                operatorStack.Push(operation);
            }
            else operatorStack.Push(operation);
        //if it's "*" "/" "("
        string[] shouldBePush = new string[] { "*", "/", "(" };
        if (shouldBePush.Contains(operation))
            operatorStack.Push(operation);
        //if it's ")"
        if (operation.Equals(")"))
        {
            if (numStack.Count > 1)
            {  //if it's "+" or "- on top
                if (operatorStack.Peek().Equals("+") || operatorStack.Peek().Equals("-"))
                    pushDoublePop();
                operatorStack.Pop();
                //if it's expression on top
                if (operatorStack.Count != 0 && operatorStack.Peek().Equals("*") || operatorStack.Count != 0 && operatorStack.Peek().Equals("/"))
                    pushDoublePop();
            }
            else operatorStack.Pop();
        }
    }
    /// <summary>
    ///  pops two operands and an operator, evaluates the operation, and pushes the result onto the operand stack
    /// </summary>
    private void pushDoublePop()
    {
        numStack.Push(evaluate(operatorStack.Pop(), numStack.Pop(), numStack.Pop()));
    }
    /// <summary>
    ///  do calculation based on the operation but have a String type
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="numA"></param>
    /// <param name="numB"></param>
    /// <returns> answer </returns>
    private double evaluate(String operation, double numB, double numA)
    {
        switch (operation)
        {
            case "+":
                return numB + numA;
            case "-":
                return numA - numB;
            case "*":
                return numB * numA;
            case "/":

                if (numB == 0)
                {
                    errorCall = true;
                    errorType = "can't divide by 0";
                    return 0;
                }
                return numA / numB;

            default: return 0;
        }
    }

}

/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    public FormulaFormatException(string message) : base(message)
    {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError
{
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason"></param>
    public FormulaError(string reason) : this()
    {
        Reason = reason;
    }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }

}



