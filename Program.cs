using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using NetSpell.SpellChecker;

namespace CryptoChallenge
{
    public class Program
    {
        private static readonly String DefaultInputPath = String.Concat(Environment.CurrentDirectory,
                                                                        Path.DirectorySeparatorChar, "input",
                                                                        Path.DirectorySeparatorChar);
        private const char Whitespace = '#'; //Saving whitespace equivalent on the encrypted text
        private static String _newLine = Environment.NewLine;

        //i'll use a string as the alphabet because i can get the elements the same way as in a List
        //and the A letter will be in position 0
        private const String Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ.,;!?";
        private static NetSpell.SpellChecker.Spelling _spell; //nuget package for detecting words

        private const Int32 NumCheckWords = 100; //maximum number of words the program will check to make a decision on key
        public static void Main(string[] args)
        {
            try
            {
                #region Variables
                String filename;
                String encryptedText;
                #region initializing spellchecker

                var dict = new NetSpell.SpellChecker.Dictionary.WordDictionary
                {
                    DictionaryFolder = String.Concat(Environment.CurrentDirectory, Path.DirectorySeparatorChar, "data"),
                    DictionaryFile = "en-US.dic"
                };
                dict.Initialize();
                _spell = new NetSpell.SpellChecker.Spelling { Dictionary = dict };

                #endregion
                #endregion

                #region Get File and Location
                if (args.Length > 0)//tries to read filename from arguments
                {
                    filename = args[0];
                }
                else//if theres no argument (double clicked exe) program need to ask for the filename
                {
                    Console.WriteLine("Enter the file name without extension (must be a .txt file)");
                    filename = Console.ReadLine();
                }

                filename += ".txt"; //gives .txt extension to the name
                var filepath = String.Concat(DefaultInputPath, filename); //concats path and filename

                #endregion
                encryptedText = File.ReadAllText(filepath); //puts all text in a single string to work with

                // gets the most suitable key for the given text
                var key = FindKey(encryptedText);

                var text = TranslateText(encryptedText, key);

                Console.WriteLine(text);

                Console.ReadKey();
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found!");
                Console.ReadKey();
            }
            catch (Exception)
            {
                Console.WriteLine("Something really unexpected happened! :( \n" +
                                            "Shutting down the application...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Finds the most suitable key for the given text
        /// The key may not be 100% compatible as there may be some character that is not in the alphabet
        /// </summary>
        /// <param name="text">String containing all the text from the file</param>
        /// <returns>The key value</returns>
        private static Int32 FindKey(String text)
        {
            // splits the text into an array using the whitespace and newline as separator.
            var words = text.Split(new char[] { Whitespace, '\r', '\n' })
                                                                .Where(word => word != string.Empty).ToArray();

            int matchNumber = -1; // this will store the number of words that match from the best key at the time
            int matchKey = -1;

            //the number of possible keys is the length of the alphabet
            for (int i = 0; i <= Alphabet.Length; i++)
            {
                //this code uses the length of the alphabet as key and tests the number of completely valid english words for the given key
                int wordsMatched = 0;

                //checks only untill reaches the end of the text or the NumCheckWords variable
                for (var j = 0; j < words.Length && j < NumCheckWords; j++)
                {
                    var word = ReplaceWithKey(words[j], i);
                    if (word.Length > 0 && _spell.TestWord(word))
                        wordsMatched++;
                }

                if (wordsMatched > matchNumber)
                {
                    matchNumber = wordsMatched;
                    matchKey = i;
                }
            }

            return matchKey;
        }

        /// <summary>
        /// Replaces the string with the given key using the alphabet
        /// </summary>
        /// <param name="word">encrypted word</param>
        /// <param name="key">given key</param>
        /// <returns>word translated with given key</returns>
        private static String ReplaceWithKey(String word, int key)
        {
            //stringbuilder who will contain the translated word
            StringBuilder wordSb = new StringBuilder();
            foreach (var character in word)
            {
                //gets the position of the current character on the alphabet String
                var alphabetPosition = Alphabet.IndexOf(character);
                if (alphabetPosition < 0)
                {
                    wordSb.Append(character);
                    continue;
                }

                wordSb.Append(GetCharPosition(alphabetPosition, key));
            }
            return wordSb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static String TranslateText(String text, int key)
        {
            StringBuilder textSb = new StringBuilder();
            foreach (var character in text)
            {
                if (Alphabet.IndexOf(character) < 0)//if its linebreak
                {
                    if (character == Whitespace)
                    {
                        textSb.Append(' ');
                    }
                    else
                    {
                        textSb.Append(character);
                    }
                }
                else
                {
                    textSb.Append(GetCharPosition(Alphabet.IndexOf(character), key));
                }
            }

            return textSb.ToString();
        }

        /// <summary>
        /// Gets the char on the normalized alphabet position
        /// </summary>
        /// <param name="alphabetPosition">current alphabet position</param>
        /// <param name="key">given key</param>
        /// <returns>translated character</returns>
        private static char GetCharPosition(int alphabetPosition, int key)
        {

            //if the position of the char minus given key results in a negative number
            //need to subtract and add the length of the alphabet
            if (alphabetPosition - key < 0)
            {
                return Alphabet[alphabetPosition - key + Alphabet.Length];
            }

            return Alphabet[alphabetPosition - key];
        }
    }

}
