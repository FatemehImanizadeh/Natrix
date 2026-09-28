using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Natrix
{
    public class NatrixInfo : GH_AssemblyInfo
    {
        public override string Name => "Natrix";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => NatrixLogo.CreateIcon(24);

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Matrix-based parking layout generation and optimization.";

        public override Guid Id => new Guid("d363a739-4da8-461e-a85d-3cf70295601c");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }

    public class NatrixAssemblyPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.ComponentServer.AddCategoryIcon("Natrix", NatrixLogo.CreateIcon(16));
            Instances.ComponentServer.AddCategorySymbolName("Natrix", 'N');
            return GH_LoadingInstruction.Proceed;
        }
    }

    internal static class NatrixLogo
    {
        internal static Bitmap CreateIcon(int size, string resourceName = "Natrix.Logo.png")
        {
            using (var stream = typeof(NatrixLogo).Assembly.GetManifestResourceStream(resourceName))
            using (var source = new Bitmap(stream))
            {
                var icon = new Bitmap(size, size);
                using (var graphics = Graphics.FromImage(icon))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.DrawImage(source, new Rectangle(0, 0, size, size));
                }
                return icon;
            }
        }
    }
}
