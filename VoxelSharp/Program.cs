namespace VoxelSharp
{
    using Common;
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            // Cool Stuff: https://www.youtube.com/watch?v=Xq3isov6mZ8
            var basedir = Utils.BaseDirectory;
            var window = new Window();
            window.Run();
            Console.WriteLine("Hello World!");
        }
    }
}
