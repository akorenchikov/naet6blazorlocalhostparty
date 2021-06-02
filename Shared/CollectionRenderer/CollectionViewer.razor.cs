using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
#pragma warning disable BL0006
namespace CollectionSettingsBeforeRender.Shared
{
    public partial class CollectionViewer {
        static RenderFragment RenderSettings(CollectionViewer self) {
            return b => {
                var frames = ExtractSettingFrames(self, b);
                int regionCount = 0;
                b.OpenRegion(0);
                b.OpenRegion(0);
                regionCount = ValidateAndRenderSettings(self, in frames, regionCount, b);
                while (regionCount-- > 0)
                    b.CloseRegion();
            };
        }

        static int ValidateAndRenderSettings(CollectionViewer self, in ArrayRange<RenderTreeFrame> frames, int regionCount, RenderTreeBuilder b)
        {
            for (int i = 0; i < frames.Count; i++)
            {
                ref var frame = ref frames.Array[i];
                switch (frame.FrameType)
                {
                    case RenderTreeFrameType.Region:
                        if (regionCount == i)
                        {
                            regionCount++;
                            b.OpenRegion(frame.Sequence);
                        }
                        break;
                    case RenderTreeFrameType.Component:
                        if (frame.ComponentType != typeof(CollectionItem))
                            throw new NotSupportedException();
                        if (i > 0)
                            b.CloseComponent();
                        b.OpenComponent(frame.Sequence, frame.ComponentType);
                        b.SetKey(frame.ComponentKey);
                        // pass through MayHaveChanges
                        b.AddAttribute(frame.Sequence, nameof(CollectionItem.Viewer), self);
                        break;
                    case RenderTreeFrameType.Attribute:
                        b.AddAttribute(frame.Sequence, frame.AttributeName, frame.AttributeValue);
                        break;
                    case RenderTreeFrameType.ComponentReferenceCapture:
                        b.AddComponentReferenceCapture(frame.Sequence, frame.ComponentReferenceCaptureAction);
                        break;
                    case RenderTreeFrameType.Element:
                    case RenderTreeFrameType.ElementReferenceCapture:
                    case RenderTreeFrameType.Text:
                        throw new NotSupportedException();
                }
            }
            if (frames.Count > 0)
                b.CloseComponent();
            return regionCount;
        }

        static ArrayRange<RenderTreeFrame> ExtractSettingFrames(CollectionViewer self, RenderTreeBuilder b)
        {
            self.Items(b);
            var frames = b.GetFrames().Clone();
            b.Clear();
            return frames;
        }
    }
}
#pragma warning restore BL0006
