# Translation Architecture
    +--------------------------+         extends           +---------------------------+
    |  ITranslationService     | <------------------------ |MicrosoftTranslationService|
    |                          |                           +---------------------------+
    |                          |
    |                          |         extends           +---------------------------+
    |                          | <------------------------ |GoogleTranslationService   |
    |                          |                           +---------------------------+
    |                          |
    |                          |         extends           +---------------------------+
    |                          | <------------------------ | NikseDkTranslationService |
    +-------------+------------+                           +---------------------------+
                  ^
                  |
                  | uses
                  |
                  |
    +-------------+--------------+        extends         +------------------------------------+
    |AbstractTranslationProcessor| <--------------------- |NextLineMergeTranslationProcessor   |
    |                            |                        +------------------------------------+
    |                            |
    |                            |        extends         +------------------------------------+
    |                            | <--------------------- |SentenceMergingTranslationProcessor |
    |                            |                        +------------------------------------+
    |                            |
    |                            |        extends         +------------------------------------+
    |                            | <--------------------- |SingleParagraphTranslationProcessor |
    +----------------------------+                        +------------------------------------+

The translation services and translation processors using the strategy pattern, to allow combination with loose code 
coupling.

##  Translation Service
Interface to allow interchangeable use of various concrete machine translation providers (google, microsoft ...). 
If you like to add another provider, simply implement it as ITranslationService to make it work with the whole 
translation infrastructure.

##  Translation Processor
handles complex translation tasks and cares about:
 - input text chunking (complying translation service specific constraints)
 - status report & cancellation option
 - how to pass the paragraphs into the translator (paragraph line handling)
 
### Paragraph Handling
Different subtitle types (e.g. film dialogues, subtitled monologue presentation videos ...) 
are machine-translated differently depending on how the paragraph lines are handled.

Current machine translation services work best with whole sentences (better than if individual 
sentence fragments are handed over separately). Unfortunately, in subtitles it is not always clear which paragraphs 
belong to a sentence, because often no separators (such as punctuation marks, question marks, explanation marks) are 
set and therefore an automatic algorithm cannot evaluate which fragments belong together and which do not.
Because of this problem, multiple translation processors with special paragraph handling strategies have been developed.

