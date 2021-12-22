using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace QuestPdf.Skia.Tizen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new QuestPdf.App(), args);
            host.Run();
        }
    }
}
