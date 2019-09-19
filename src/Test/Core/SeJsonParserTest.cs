using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;

namespace Test.Core
{
    [TestClass]
    public class SeJsonParserTest
    {
        [TestMethod]
        public void Simple()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("{ \"content\" : \"Joe\"}", "content");
            Assert.AreEqual("Joe", result.First());
        }

        [TestMethod]
        public void SimpleQuote()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("{ \"content\" : \"Joe \\\"is\\\" best\"}", "content");
            Assert.AreEqual("Joe \\\"is\\\" best", result.First());
        }

        [TestMethod]
        public void SimpleArray()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("[{ \"content\" : \"Joe1\"},{ \"content\" : \"Joe2\"}]", "content");
            Assert.AreEqual("Joe1", result[0]);
            Assert.AreEqual("Joe2", result[1]);
        }

        [TestMethod]
        public void Complex()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("{" + Environment.NewLine +
                                                          "\"name\":\"John\"," + Environment.NewLine +
                                                          "\"age\":30," + Environment.NewLine +
                                                          "\"cars\": [" + Environment.NewLine +
                                                          "{ \"name\":\"Ford\", \"content\":\"Fiesta\"  }," + Environment.NewLine +
                                                          "{ \"name\":\"BMW\", \"content\": \"X3\"}," + Environment.NewLine +
                                                          "{ \"name\":\"Fiat\", \"content\": \"500\" } ]}", "content");
            Assert.AreEqual("Fiesta", result[0]);
            Assert.AreEqual("X3", result[1]);
            Assert.AreEqual("500", result[2]);
        }

        [TestMethod]
        public void SimpleNumber()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("[{ \"content\" : 10 }]", "content");
            Assert.AreEqual("10", result[0]);
        }

        [TestMethod]
        public void SimpleBoolTrue()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("[{ \"content\" : true }]", "content");
            Assert.AreEqual("true", result[0]);
        }

        [TestMethod]
        public void SimpleBoolFalse()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("[{ \"content\" : false }]", "content");
            Assert.AreEqual("false", result[0]);
        }

        [TestMethod]
        public void SimpleNull()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("[{ \"content\" : null }]", "content");
            Assert.AreEqual(null, result[0]);
        }


        [TestMethod]
        public void GetArrayElementsByName_Simple()
        {
            var parser = new SeJsonParser();
            var result = parser.GetArrayElementsByName("{ \"items\": [ { \"name\" : \"Joe1\" },{ \"name\" : \"Joe2\" } ] }", "items");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("{ \"name\" : \"Joe1\" }", result[0].Trim());
            Assert.AreEqual("{ \"name\" : \"Joe2\" }", result[1].Trim());
        }

        [TestMethod]
        public void GetArrayElementsByName_Simple_Compact()
        {
            var parser = new SeJsonParser();
            var result = parser.GetArrayElementsByName("{\"items\":[{\"name\":\"Joe1\"},{\"name\":\"Joe2\"}]}", "items");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("{\"name\":\"Joe1\"}", result[0].Trim());
            Assert.AreEqual("{\"name\":\"Joe2\"}", result[1].Trim());
        }

        [TestMethod]
        public void GetArrayElementsByName_Empty_Array()
        {
            var parser = new SeJsonParser();
            var result = parser.GetArrayElementsByName("{ \"start_time\": \"118.64\", \"items\": [] }", "items");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("", result[0].Trim());
        }

        [TestMethod]
        public void GetArrayElementsByName_Advanced()
        {
            var parser = new SeJsonParser();
            var result = parser.GetArrayElementsByName(@"{
    'results': {
        'transcripts': [
            {
                'transcript': 'em águas brasileiras estão achado mais antigo que o homem. Pré sal pré sal começa a se formar há mais de cem milhões de Jonas com Regina. Salas ainda habitavam até os postos são de altíssima produtividade. As grandes reservas de petróleo foram descobertas em dois mil e seis. Dois anos depois, o Brasil já começava a produzir petróleo retirado do pré sal. E hoje, quando a gente olha para trás, era muito desconhecido. Já são dez anos, vem sendo desafios para isso. As equipes contam com tecnologia especialmente desenvolvida para o pré sal. Muitas soluções saíram do Centro de Pesquisas da Petrobras Cenpes, no Rio de Janeiro. Aqui, pesquisadores inventaram técnicas que não existiam no mercado, coisa que o geofísico desenvolveram técnicas de processamento muito especial de matemática pesada. Para tentar mostrar de maneira mais óbvia onde estava roxa de pessoal, a área do pré sal fica a trezentos quilômetros da costa que vai do Espírito Santo a Santa Catarina, uma extensão de oitocentos quilômetros. O petróleo está a sete mil metros de profundidade, abaixo de uma espessa camada de sal, que funciona como um sino. Ao contrário do que muita gente acha que o petróleo tem, piscinas, debaixo do Mar Petróleo se aloja nesses buracos nessas horas, muitas dessas rochas estão no fundo da Bacia de Santos, no litoral de São Paulo. Metade da produção de petróleo que a gente tem aqui no Brasil já vem do pré sal, e um terço do gás que é consumido pelo Brasil também vem do pré sal. Então, se a gente não tivesse descoberto e desenvolvido essa tecnologia toda para poder operar as plataformas, hoje a gente estaria muito mais dependente do mercado externo. A jornada do conhecimento da tecnologia trouxe para a Petrobras a inovação que certamente para nós, hoje são ativos, são mais importantes do que o petróleo.'
            }
        ],
        'speaker_labels': {
            'speakers': 4,
            'segments': [
                {
                    'start_time': '5.17',
                    'speaker_label': 'spk_0',
                    'end_time': '9.25',
                    'items': [
                        {
                            'start_time': '5.17',
                            'speaker_label': 'spk_0',
                            'end_time': '5.37'
                        },
                        {
                            'start_time': '105.54',
                            'speaker_label': 'spk_1',
                            'end_time': '106.09'
                        }
                    ]
                }
            ]
        },
        'items': [
            {
                'start_time': '5.17',
                'end_time': '5.37',
                'alternatives': [
                    {
                        'confidence': '0.9999',
                        'content': 'em'
                    }
                ],
                'type': 'pronunciation'
            },
            {
                'start_time': '5.37',
                'end_time': '5.67',
                'alternatives': [
                    {
                        'confidence': '0.9999',
                        'content': 'águas'
                    }
                ],
                'type': 'pronunciation'
            }
        ]
    },
    'status': 'COMPLETED'
}".Replace('\'', '"'), "items");
            Assert.AreEqual(4, result.Count);
        }
    }
}