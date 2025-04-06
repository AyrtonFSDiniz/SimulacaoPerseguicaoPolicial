public static class ConsoleLog
{
    public static void Log(string mensagem)
    {
        var logMessage = $"{DateTime.Now}: {mensagem}";
        Console.WriteLine(logMessage);
        Console.WriteLine("\n");
    }
}