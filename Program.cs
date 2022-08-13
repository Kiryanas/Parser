using System;
using System.IO;

class ParserDemo
{
    public static void Main()
    {
        string expr;
        Parser p = new Parser();
        Console.WriteLine(
        "Для выхода из программы введите пустое выражение.");
        for (; ; )
        {
            Console.Write("Введите выражение: ");
            expr = Console.ReadLine();
            if (expr == "") break;
            Console.WriteLine("Результат: " + p.Evaluate(expr));
        }
    }
}