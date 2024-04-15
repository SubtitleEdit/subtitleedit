using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.TextToSpeech
{
    public class ElevelLabModels
    {
        public string Voice { get; set; }
        public string Language { get; set; }
        public string Gender { get; set; }
        public string Model { get; set; }

        public override string ToString()
        {
            return $"{Language} - {Voice} ({Gender})";
        }

        public ElevelLabModels(string language, string voice, string gender, string description, string useCase, string accent, string voiceId)
        {
            Voice = voice;
            Language = accent;
            Gender = Gender;
            Model = voiceId;
        }

        public static List<ElevelLabModels> GetVoices()
        {
            var models = new List<ElevelLabModels>
            {
                new ElevelLabModels("English", "Adam", "Male", "Deep", "Narration", "American English", "pNInz6obpgDQGcFmaJgB"),
                new ElevelLabModels("English", "Charlie", "Male", "Casual", "Conversational", "Australian English", "IKne3meq5aSn9XLyUdCD"),
                new ElevelLabModels("English", "Clyde", "Male", "War veteran", "Video games", "American English", "2EiwWnXFnvU5JabPnv8n"),
                new ElevelLabModels("English", "Dorothy", "Female", "Pleasant", "Children’s stories", "British English", "ThT5KcBeYPX3keUQqHPh"),
                new ElevelLabModels("English", "Freya", "Female", "Overhyped", "Video games", "American English", "jsCqWAovK2LkecY7zXl4"),
                new ElevelLabModels("English", "Gigi", "Female", "Childlish", "Animation", "American English", "jBpfuIE2acCO8z3wKNLl"),
                new ElevelLabModels("English", "Harry", "Male", "Anxious", "Video games", "American English", "SOYHLrjzK2X1ezoPC6cr"),
                new ElevelLabModels("English", "James", "Male", "Calm", "News", "Australian English", "ZQe5CZNOzWyzPSCn5a3c"),
                new ElevelLabModels("English", "Lily", "Female", "Raspy", "Narration", "British English", "pFZP5JQG7iQjIQuC4Bku"),
                new ElevelLabModels("English", "Rachel", "Female", "Calm", "Narration", "American English", "21m00Tcm4TlvDq8ikWAM"),
                new ElevelLabModels("Spanish", "Dorothy", "Female", "Pleasant", "News", "Chilean Spanish", "ThT5KcBeYPX3keUQqHPh"),
                new ElevelLabModels("Spanish", "Glinda", "Female", "Witch", "Video games", "Mexican Spanish", "z9fAnlkpzviPz146aGWa"),
                new ElevelLabModels("Spanish", "Grace", "Female", "gentle", "Audiobook", "Mexican Spanish", "oWAxZDx7w5VEj9dCyTzz"),
                new ElevelLabModels("Spanish", "Matilda", "Female", "Warm", "Audiobook", "Chilean Spanish", "XrExE9yKIg1WjnnlVkGX"),
                new ElevelLabModels("German", "Sarah", "Female", "Soft", "News", "Germany German", "EXAVITQu4vr4xnSDxMaL"),
                new ElevelLabModels("German", "Serena", "Female", "Pleasant", "Interactive", "Germany German", "pMsXgVXv3BLzUgSXRplE"),
                new ElevelLabModels("German", "Matilda", "Female", "Warm", "Audiobook", "Germany German", "XrExE9yKIg1WjnnlVkGX"),
                new ElevelLabModels("German", "Freya", "Female", "Overhyped", "Video games", "Germany German", "jsCqWAovK2LkecY7zXl4"),
                new ElevelLabModels("German", "Adam", "Male", "Deep", "Narration", "Germany German", "pNInz6obpgDQGcFmaJgB"),
                new ElevelLabModels("German", "Antoni", "Male", "Well-rounded", "Narration", "Germany German", "ErXwobaYiN019PkySvjV"),
                new ElevelLabModels("French", "Adam", "Male", "Deep", "Narration", "Canadian French", "pNInz6obpgDQGcFmaJgB"),
                new ElevelLabModels("French", "Antoni", "Male", "Well-rounded", "Narration", "Canadian French", "ErXwobaYiN019PkySvjV"),
                new ElevelLabModels("French", "Arnold", "Male", "Crisp", "Narration", "Canadian French", "VR6AewLTigWG4xSOukaG"),
                new ElevelLabModels("French", "Bill", "Male", "Strong", "documentary", "Canadian French", "pqHfZKP75CvOlQylNhV4"),
                new ElevelLabModels("French", "George", "Male", "Raspy", "Narration", "Canadian French", "JBFqnCBsd6RMkjVDRZzb"),
                new ElevelLabModels("French", "Charlotte", "Female", "Seductive", "Video games", "Canadian French", "XB0fDUnXU5powFXDhCwa"),
                new ElevelLabModels("French", "Domi", "Female", "Strong", "Narration", "Canadian French", "AZnzlk1XvdvUeBnXmlld"),
                new ElevelLabModels("French", "Dorothy", "Female", "Pleasant", "Children’s stories", "Canadian French", "ThT5KcBeYPX3keUQqHPh"),
                new ElevelLabModels("French", "Serena", "Female", "Pleasant", "Interactive", "Canadian French", "pMsXgVXv3BLzUgSXRplE"),
                new ElevelLabModels("French", "Sarah", "Female", "Soft", "News", "Canadian French", "EXAVITQu4vr4xnSDxMaL"),
                new ElevelLabModels("Polish", "Adam", "Male", "Deep", "Narration", "Poland Polish", "pNInz6obpgDQGcFmaJgB"),
                new ElevelLabModels("Polish", "Charlie", "Male", "Casual", "Conversational", "Poland Polish", "IKne3meq5aSn9XLyUdCD"),
                new ElevelLabModels("Polish", "Clyde", "Male", "War veteran", "video games", "Poland Polish", "2EiwWnXFnvU5JabPnv8n"),
                new ElevelLabModels("Polish", "Dorothy", "Female", "Pleasant", "Children’s stories", "Poland Polish", "ThT5KcBeYPX3keUQqHPh"),
                new ElevelLabModels("Polish", "Gigi", "Female", "Childlish", "Animation", "Poland Polish", "jBpfuIE2acCO8z3wKNLl"),
                new ElevelLabModels("Polish", "Harry", "Male", "Anxious", "Video games", "Poland Polish", "SOYHLrjzK2X1ezoPC6cr"),
                new ElevelLabModels("Italian", "Adam", "Male", "Deep", "Narration", "Italy Italian", "pNInz6obpgDQGcFmaJgB"),
                new ElevelLabModels("Italian", "Charlie", "Male", "Casual", "Conversational", "Italy Italian", "IKne3meq5aSn9XLyUdCD"),
                new ElevelLabModels("Italian", "Clyde", "Male", "War veteran", "Video games", "Italy Italian", "2EiwWnXFnvU5JabPnv8n"),
                new ElevelLabModels("Italian", "Dorothy", "Female", "Pleasant", "Children’s stories", "Italy Italian", "ThT5KcBeYPX3keUQqHPh"),
                new ElevelLabModels("Italian", "Gigi", "Female", "Childlish", "Animation", "Italy Italian", "jBpfuIE2acCO8z3wKNLl"),
                new ElevelLabModels("Italian", "Harry", "Male", "Anxious", "Video games", "Italy Italian", "SOYHLrjzK2X1ezoPC6cr"),
            };

            return models;
        }
    }
}