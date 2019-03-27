using System.Collections.Generic;
using System.Linq;

namespace Tools
{
    /// <summary>
    /// Represents a stack of labels that are appended to IP package
    /// and methods that are applicable to it.
    /// </summary>
    public class LabelStack
    {
        /// <summary>
        /// A stack of labels appended to IP package.
        /// </summary>
        public Stack<Label> Labels { get; set; }

        /// <summary>
        /// This value indicates that the stack is empty.
        /// </summary>
        public const byte StackEmptyFlag = 0x00;

        /// <summary>
        /// This flag indicates, that the stack is not empty.
        /// </summary>
        public const byte StackNotEmptyFlag = 0xff;

        /// <summary>
        /// Initalizes a stack of labels.
        /// </summary>
        public LabelStack()
        {
            Labels = new Stack<Label>();
        }

        /// <summary>
        /// Converts a stack of labels to a byte form.
        /// </summary>
        /// <returns>Converted label stack</returns>
        public List<byte> GetBytes()
        {
            var bytes = new List<byte>();
            var stackFlag = IsEmpty() ? StackEmptyFlag : StackNotEmptyFlag;
            bytes.AddRange(new List<byte>() { stackFlag });

            var labels = Labels.ToList();
            for (int i = 0; i < labels.Count; i++)
            {
                bytes.AddRange(labels[i].GetBytes());
                if (i != labels.Count - 1)
                {
                    bytes.Add(0x00);
                }
                else
                {
                    bytes.Add(0xff);
                }
            }
            return bytes;
        }

        /// <summary>
        /// Checks if label stack is empty.
        /// </summary>
        /// <returns>True, if is empty; false otherwise</returns>
        public bool IsEmpty()
        {
            return Labels.Count == 0;
        }

        /// <summary>
        /// Converts a stream of bytes to a valid LabelStack.
        /// </summary>
        /// <param name="bytes">A stream of bytes that will be converted.</param>
        /// <returns>Valid label stack</returns>
        public static LabelStack FromBytes(byte[] bytes)
        {
            LabelStack labelStack = new LabelStack();

            if (bytes[0] == StackNotEmptyFlag)
            {
                var index = 1;
                while (true)
                {
                    var label = new Label
                    {
                        Number = (short)((bytes[index + 1] << 8) + bytes[index]),
                        TTL = bytes[index + 2]
                    };
                    var bottomOfStack = bytes[index + 3];

                    labelStack.Labels.Push(label);
                    if (bottomOfStack == 0xff)
                    {
                        break;
                    }

                    index = index + 4;
                }
            }
            return labelStack;
        }

        /// <summary>
        /// Gets a length of label stack (in bytes).
        /// </summary>
        /// <returns>Stack's length in bytes.</returns>
        public int GetLength()
        {
            return IsEmpty() ? 1 : (1 + Labels.Count * 4);
        }

        /// <summary>
        /// Generates a string consisting all labels' numbers seperated by a comma.
        /// </summary>
        /// <returns>Generated string.</returns>
        public override string ToString()
        {
            return string.Join(", ", Labels.ToList().Select(label => label.Number));
        }
    }
}
