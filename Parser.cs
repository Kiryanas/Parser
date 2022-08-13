using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Класс исключений для обнаружения ошибок анализатора. 
class ParserException : ApplicationException
{
    public ParserException(string str) : base(str) { }
    public override string ToString()
    {
        return Message;
    }
}

class Parser
    {
        // Перечисляем типы лексем. 
        enum Types { NONE, DELIMITER, VARIABLE, NUMBER };
        // Перечисляем типы ошибок. 
        enum Errors { SYNTAX, UNBALPARENS, NOEXP, DIVBYZERO };
        string exp; // Ссылка на строку выражения. 
        int expIdx; // Текущий индекс в выражении. 
        string token; // Текущая лексема. 
        Types tokType; // Тип лексемы. 
                       // Входная точка анализатора. 
        public double Evaluate(string expstr)
        {
            double result;
            exp = expstr;
            expIdx = 0;
            try
            {
                GetToken();
                if (token == "")
                {
                    SyntaxErr(Errors.NOEXP); // Выражение отсутствует. 
                    return 0.0;
                }
                EvalExp2(out result);
                if (token != "") // Последняя лексема должна быть null-значением. 
                    SyntaxErr(Errors.SYNTAX);
                return result;
            }
            catch (ParserException exc)
            {
                Console.WriteLine(exc);
            return 0.0;
            }
        }
        // Сложение или вычитание двух членов выражения. 
        void EvalExp2(out double result)
        {
            string op;
            double partialResult;
            EvalExp3(out result);
            while ((op = token) == "+" || op == "-")
            {
                GetToken();
                EvalExp3(out partialResult);
                switch (op)
                {
                    case "-":
                        result = result - partialResult;
                        break;
                    case "+":
                        result = result + partialResult;
                        break;
                }
            }
        }
        // Умножение или деление двух множителей. 
        void EvalExp3(out double result)
        {
            string op;
            double partialResult = 0.0;
            EvalExp4(out result);
            while ((op = token) == "*" ||
            op == "/" || op == "%")
            {
                GetToken();
                EvalExp4(out partialResult);
                switch (op)
                {
                    case "*":
                        result = result * partialResult;
                        break;
                    case "/":
                        if (partialResult == 0.0)
                            SyntaxErr(Errors.DIVBYZERO);
                        result = result / partialResult;
                        break;
                    case "%":
                        if (partialResult == 0.0)
                            SyntaxErr(Errors.DIVBYZERO);
                        result = (int)result % (int)partialResult;
                        break;
                }
            }
        }
        // Возведение в степень. 
        void EvalExp4(out double result)
        {
            double partialResult, ex;
            int t;
            EvalExp5(out result);
            if (token == "^")
            {
                GetToken();
                EvalExp4(out partialResult);
                ex = result;
                if (partialResult == 0.0)
                {
                    result = 1.0;
                    return;
                }
                for (t = (int)partialResult - 1; t > 0; t--)
                    result = result * (double)ex;
            }
        }
        // Выполнение операции унарного + или -. 
        void EvalExp5(out double result)
        {
            string op;
            op = "";
            if ((tokType == Types.DELIMITER) &&
            token == "+" || token == "-")
            {
                op = token;
                GetToken();
            }
            EvalExp6(out result);
            if (op == "-") result = -result;
        }
        // Обработка выражения в круглых скобках. 
        void EvalExp6(out double result)
        {
            if ((token == "("))
            {
                GetToken();
                EvalExp2(out result);
                if (token != ")")
                    SyntaxErr(Errors.UNBALPARENS);
                GetToken();
            }
            else Atom(out result);
        }
        // Получаем значение числа. 
        void Atom(out double result)
        {
            switch (tokType)
            {
                case Types.NUMBER:
                    try
                    {
                        result = Double.Parse(token);
                    }
                    catch (FormatException)
                    {
                        result = 0.0;
                        SyntaxErr(Errors.SYNTAX);
                    }
                    GetToken();
                    return;
                default:
                    result = 0.0;
                    SyntaxErr(Errors.SYNTAX);
                    break;
            }
        }
        
        // Обрабатываем синтаксическую ошибку. 
        void SyntaxErr(Errors error)
        {
               string[] err = {
                 "Синтаксическая ошибка",
                 "Дисбаланс скобок",
                 "Выражение отсутствует",
                 "Деление на нуль"
                };
            throw new ParserException(err[(int)error]);
        }
        
        // Получаем следующую лексему. 
        void GetToken()
        {
            tokType = Types.NONE;
            token = "";
            if (expIdx == exp.Length) return; // конец выражения
            // Пропускаем пробелы. 
            while (expIdx < exp.Length &&
            Char.IsWhiteSpace(exp[expIdx])) ++expIdx;
            // Хвостовой пробел завершает выражение. 
            if (expIdx == exp.Length) return;
            if (IsDelim(exp[expIdx]))
            { // Это оператор? 
                token += exp[expIdx];
                expIdx++;
                tokType = Types.DELIMITER;
            }
            else if (Char.IsLetter(exp[expIdx]))
            { // Это
              // переменная? 
                while (!IsDelim(exp[expIdx]))
                {
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length)
                        break;
                }
                tokType = Types.VARIABLE;
            }
            else if (Char.IsDigit(exp[expIdx]))
            { // Это число? 
                while (!IsDelim(exp[expIdx]))
                {
                    token += exp[expIdx];
                    expIdx++;
                    if (expIdx >= exp.Length)
                        break;
                }
                tokType = Types.NUMBER;
            }
        }
        // Метод возвращает значение true, если символ с является разделителем. 
        bool IsDelim(char c)
        {
            if ((" +-/*%^=()".IndexOf(c) != -1)) return true;
            return false;
        }
}
