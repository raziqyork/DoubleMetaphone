namespace System.Text
{
    #region Using directives

    using RegularExpressions;

    #endregion

    public static class DoubleMetaphone
    {
        public static Result GetDoubleMetaphone(this string text)
        {
            if (text == null) throw new NullReferenceException();

            string primary, secondary;
            if (!TryParse(text, out primary, out secondary))
                throw new InvalidOperationException("Could not calculate DoubleMetaphone.");

            return new Result(primary, secondary);
        }

        public static bool TryParse(string text, out string primary, out string secondary)
        {
            primary = "";
            secondary = "";
            var current = 0;
            var length = text.Length;
            var lastLength = length - 1;
            var original = text.ToUpperInvariant() + "     ";

            // skip this at beginning of word
            if (SubstringMatches(original, 0, 2, "GN", "KN", "PN", "WR", "PS"))
            {
                current++;
            }

            // Initial 'X' is pronounced 'Z' e.g. "Xavier"
            if (original[0] == 'X')
            {
                primary += 'S'; // 'Z' maps to 'S'
                secondary += 'S';
                current++;
            }

            // main loop
            while (primary.Length < 4 || secondary.Length < 4)
            {
                if (current >= length)
                {
                    break;
                }

                switch (original[current])
                {
                    case 'A':
                    case 'E':
                    case 'I':
                    case 'O':
                    case 'U':
                    case 'Y':
                        if (current == 0)
                        {
                            // all init vowels now map to 'A'
                            primary += 'A';
                            secondary += 'A';
                        }
                        ++current;
                        break;

                    case 'B':
                        // "-mb", e.g. "dumb", already skipped over ...
                        primary += 'P';
                        secondary += 'P';

                        if (original[current + 1] == 'B')
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        break;

                    //case 'Ã': //Ã‡
                    //    _primary += 'S';
                    //    _secondary += 'S';
                    //    ++_current;
                    //    break;

                    case 'C':
                        // various gremanic
                        if (current > 1
                            && !IsVowel(original, current - 2)
                            && SubstringMatches(original, current - 1, 3, "ACH")
                            && (
                                (original[current + 2] != 'I')
                                && (
                                    (original[current + 2] != 'E')
                                    || SubstringMatches(original, current - 2, 6, "BACHER", "MACHER")
                                    )
                                )
                            )
                        {
                            primary += 'K';
                            secondary += 'K';
                            current += 2;
                            break;
                        }

                        // special case "caesar"
                        if (current == 0
                            && SubstringMatches(original, current, 6, "CAESAR")
                            )
                        {
                            primary += 'S';
                            secondary += 'S';
                            current += 2;
                            break;
                        }

                        // italian "chianti"
                        if (SubstringMatches(original, current, 4, "CHIA"))
                        {
                            primary += 'K';
                            secondary += 'K';
                            current += 2;
                            break;
                        }

                        if (SubstringMatches(original, current, 2, "CH"))
                        {
                            // find "michael"
                            if (current > 0
                                && SubstringMatches(original, current, 4, "CHAE")
                                )
                            {
                                primary += 'K';
                                secondary += 'X';
                                current += 2;
                                break;
                            }

                            // greek roots e.g. "chemistry", "chorus"
                            if (current == 0
                                && (
                                    SubstringMatches(original, current + 1, 5, "HARAC", "HARIS")
                                    || SubstringMatches(original, current + 1, 3, "HOR", "HYM", "HIA", "HEM")
                                    )
                                && !SubstringMatches(original, 0, 5, "CHORE")
                                )
                            {
                                primary += 'K';
                                secondary += 'K';
                                current += 2;
                                break;
                            }

                            // germanic, greek, or otherwise "ch" for "kh" sound
                            if ((
                                SubstringMatches(original, 0, 4, "VAN ", "VON ")
                                || SubstringMatches(original, 0, 3, "SCH")
                                )
                                // "architect" but not "arch", orchestra", "orchid"
                                || SubstringMatches(original, current - 2, 6, "ORCHES", "ARCHIT", "ORCHID")
                                || SubstringMatches(original, current + 2, 'T', 'S')
                                || (
                                    (
                                        SubstringMatches(original, current - 1, 'A', 'O', 'U', 'E')
                                        || current == 0
                                        )
                                // e.g. "wachtler", "weschsler", but not "tichner"
                                    &&
                                    SubstringMatches(original, current + 2,
                                        'L', 'R', 'N', 'M', 'B', 'H', 'F', 'V', 'W', ' ')
                                    )
                                )
                            {
                                primary += 'K';
                                secondary += 'K';
                            }
                            else
                            {
                                if (current > 0)
                                {
                                    if (SubstringMatches(original, 0, 2, "MC"))
                                    {
                                        // e.g. "McHugh"
                                        primary += 'K';
                                        secondary += 'K';
                                    }
                                    else
                                    {
                                        primary += 'X';
                                        secondary += 'K';
                                    }
                                }
                                else
                                {
                                    primary += 'X';
                                    secondary += 'X';
                                }
                            }
                            current += 2;
                            break;
                        }

                        // e.g. "czerny"
                        if (SubstringMatches(original, current, 2, "CZ")
                            && !SubstringMatches(original, current - 2, 4, "WICZ")
                            )
                        {
                            primary += 'S';
                            secondary += 'X';
                            current += 2;
                            break;
                        }

                        // e.g. "focaccia"
                        if (SubstringMatches(original, current + 1, 3, "CIA"))
                        {
                            primary += 'X';
                            secondary += 'X';
                            current += 3;
                            break;
                        }

                        // double 'C', but not McClellan"
                        if (SubstringMatches(original, current, 2, "CC")
                            && !(
                                current == 1
                                && original[0] == 'M'
                                )
                            )
                        {
                            // "bellocchio" but not "bacchus"
                            if (SubstringMatches(original, current + 2, 'I', 'E', 'H')
                                && !SubstringMatches(original, current + 2, 2, "HU")
                                )
                            {
                                // "accident", "accede", "succeed"
                                if ((
                                    current == 1
                                    && original[current - 1] == 'A'
                                    )
                                    || SubstringMatches(original, current - 1, 5, "UCCEE", "UCCES")
                                    )
                                {
                                    primary += "KS";
                                    secondary += "KS";
                                    // "bacci", "bertucci", other italian
                                }
                                else
                                {
                                    primary += 'X';
                                    secondary += 'X';
                                }
                                current += 3;
                                break;
                            }
                            // Pierce"s rule
                            primary += 'K';
                            secondary += 'K';
                            current += 2;
                            break;
                        }

                        if (SubstringMatches(original, current, 2, "CK", "CG", "CQ"))
                        {
                            primary += 'K';
                            secondary += 'K';
                            current += 2;
                            break;
                        }

                        if (SubstringMatches(original, current, 2, "CI", "CE", "CY"))
                        {
                            // italian vs. english
                            if (SubstringMatches(original, current, 3, "CIO", "CIE", "CIA"))
                            {
                                primary += 'S';
                                secondary += 'X';
                            }
                            else
                            {
                                primary += 'S';
                                secondary += 'S';
                            }
                            current += 2;
                            break;
                        }

                        // else
                        primary += 'K';
                        secondary += 'K';

                        // name sent in "mac caffrey", "mac gregor"
                        if (SubstringMatches(original, current + 1, 2, " C", " Q", " G"))
                        {
                            current += 3;
                        }
                        else
                        {
                            if (SubstringMatches(original, current + 1, 'C', 'K', 'Q')
                                && !SubstringMatches(original, current + 1, 2, "CE", "CI")
                                )
                            {
                                current += 2;
                            }
                            else
                            {
                                ++current;
                            }
                        }
                        break;

                    case 'D':
                        if (SubstringMatches(original, current, 2, "DG"))
                        {
                            if (SubstringMatches(original, current + 2, 'I', 'E', 'Y'))
                            {
                                // e.g. "edge"
                                primary += 'J';
                                secondary += 'J';
                                current += 3;
                                break;
                            }
                            // e.g. "edgar"
                            primary += "TK";
                            secondary += "TK";
                            current += 2;
                            break;
                        }

                        if (SubstringMatches(original, current, 2, "DT", "DD"))
                        {
                            primary += 'T';
                            secondary += 'T';
                            current += 2;
                            break;
                        }

                        // else
                        primary += 'T';
                        secondary += 'T';
                        ++current;
                        break;

                    case 'F':
                        if (original[current + 1] == 'F')
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        primary += 'F';
                        secondary += 'F';
                        break;

                    case 'G':
                        if (original[current + 1] == 'H')
                        {
                            if (current > 0
                                && !IsVowel(original, current - 1)
                                )
                            {
                                primary += 'K';
                                secondary += 'K';
                                current += 2;
                                break;
                            }

                            if (current < 3)
                            {
                                // "ghislane", "ghiradelli"
                                if (current == 0)
                                {
                                    if (original[current + 2] == 'I')
                                    {
                                        primary += 'J';
                                        secondary += 'J';
                                    }
                                    else
                                    {
                                        primary += 'K';
                                        secondary += 'K';
                                    }
                                    current += 2;
                                    break;
                                }
                            }

                            // Parker"s rule (with some further refinements) - e.g. "hugh"
                            if ((
                                current > 1
                                && SubstringMatches(original, current - 2, 'B', 'H', 'D')
                                )
                                // e.g. "bough"
                                || (
                                    current > 2
                                    && SubstringMatches(original, current - 3, 'B', 'H', 'D')
                                    )
                                // e.g. "broughton"
                                || (
                                    current > 3
                                    && SubstringMatches(original, current - 4, 'B', 'H')
                                    )
                                )
                            {
                                current += 2;
                                break;
                            }
                            // e.g. "laugh", "McLaughlin", "cough", "gough", "rough", "tough"
                            if (current > 2
                                && original[current - 1] == 'U'
                                && SubstringMatches(original, current - 3, 'C', 'G', 'L', 'R', 'T')
                                )
                            {
                                primary += 'F';
                                secondary += 'F';
                            }
                            else if (
                                current > 0
                                && original[current - 1] != 'I'
                                )
                            {
                                primary += 'K';
                                secondary += 'K';
                            }
                            current += 2;
                            break;
                        }

                        if (original[current + 1] == 'N')
                        {
                            if (current == 1
                                && IsVowel(original, 0)
                                && !IsSlavoGermanic(original)
                                )
                            {
                                primary += "KN";
                                secondary += 'N';
                            }
                            else
                            {
                                // not e.g. "cagney"
                                if (!SubstringMatches(original, current + 2, 2, "EY")
                                    && original[current + 1] != 'Y'
                                    && !IsSlavoGermanic(original)
                                    )
                                {
                                    primary += 'N';
                                    secondary += "KN";
                                }
                                else
                                {
                                    primary += "KN";
                                    secondary += "KN";
                                }
                            }
                            current += 2;
                            break;
                        }

                        // "tagliaro"
                        if (SubstringMatches(original, current + 1, 2, "LI")
                            && !IsSlavoGermanic(original)
                            )
                        {
                            primary += "KL";
                            secondary += 'L';
                            current += 2;
                            break;
                        }

                        // -ges-, -gep-, -gel- at beginning
                        if (current == 0
                            && (
                                original[current + 1] == 'Y'
                                ||
                                SubstringMatches(original, current + 1, 2,
                                    "ES", "EP", "EB", "EL", "EY", "IB", "IL", "IN", "IE", "EI", "ER")
                                )
                            )
                        {
                            primary += 'K';
                            secondary += 'J';
                            current += 2;
                            break;
                        }

                        // -ger-, -gy-
                        if ((
                            SubstringMatches(original, current + 1, 2, "ER")
                            || original[current + 1] == 'Y'
                            )
                            && !SubstringMatches(original, 0, 6, "DANGER", "RANGER", "MANGER")
                            && !SubstringMatches(original, current - 1, 'E', 'I')
                            && !SubstringMatches(original, current - 1, 3, "RGY", "OGY")
                            )
                        {
                            primary += 'K';
                            secondary += 'J';
                            current += 2;
                            break;
                        }

                        // italian e.g. "biaggi"
                        if (SubstringMatches(original, current + 1, 'E', 'I', 'Y')
                            || SubstringMatches(original, current - 1, 4, "AGGI", "OGGI")
                            )
                        {
                            // obvious germanic
                            if ((
                                SubstringMatches(original, 0, 4, "VAN ", "VON ")
                                || SubstringMatches(original, 0, 3, "SCH")
                                )
                                || SubstringMatches(original, current + 1, 2, "ET")
                                )
                            {
                                primary += 'K';
                                secondary += 'K';
                            }
                            else
                            {
                                // always soft if french ending
                                if (SubstringMatches(original, current + 1, 4, "IER "))
                                {
                                    primary += 'J';
                                    secondary += 'J';
                                }
                                else
                                {
                                    primary += 'J';
                                    secondary += 'K';
                                }
                            }
                            current += 2;
                            break;
                        }

                        if (original[current + 1] == 'G')
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }

                        primary += 'K';
                        secondary += 'K';
                        break;

                    case 'H':
                        // only keep if first & before vowel or btw. 2 vowels
                        if ((
                            current == 0
                            || IsVowel(original, current - 1)
                            )
                            && IsVowel(original, current + 1)
                            )
                        {
                            primary += 'H';
                            secondary += 'H';
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        break;

                    case 'J':
                        // obvious spanish, "jose", "san jacinto"
                        if (SubstringMatches(original, current, 4, "JOSE")
                            || SubstringMatches(original, 0, 4, "SAN ")
                            )
                        {
                            if ((
                                current == 0
                                && Substring(original, current + 4, 1) == " "
                                )
                                || SubstringMatches(original, 0, 4, "SAN ")
                                )
                            {
                                primary += 'H';
                                secondary += 'H';
                            }
                            else
                            {
                                primary += 'J';
                                secondary += 'H';
                            }
                            ++current;
                            break;
                        }

                        if (current == 0
                            && !SubstringMatches(original, current, 4, "JOSE")
                            )
                        {
                            primary += 'J'; // Yankelovich/Jankelowicz
                            secondary += 'A';
                        }
                        else
                        {
                            // spanish pron. of .e.g. "bajador"
                            if (IsVowel(original, current - 1)
                                && !IsSlavoGermanic(original)
                                && (
                                    original[current + 1] == 'A'
                                    || original[current + 1] == 'O'
                                    )
                                )
                            {
                                primary += 'J';
                                secondary += 'H';
                            }
                            else
                            {
                                if (current == lastLength)
                                {
                                    primary += 'J';
                                    // _secondary += "";
                                }
                                else
                                {
                                    if (!SubstringMatches(original, current + 1,
                                        'L', 'T', 'K', 'S', 'N', 'M', 'B', 'Z')
                                        && !SubstringMatches(original, current - 1, 'S', 'K', 'L')
                                        )
                                    {
                                        primary += 'J';
                                        secondary += 'J';
                                    }
                                }
                            }
                        }

                        if (original[current + 1] == 'J')
                        {
                            // it could happen
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        break;

                    case 'K':
                        if (original[current + 1] == 'K')
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        primary += 'K';
                        secondary += 'K';
                        break;

                    case 'L':
                        if (original[current + 1] == 'L')
                        {
                            // spanish e.g. "cabrillo", "gallegos"
                            if ((
                                current == (length - 3)
                                && SubstringMatches(original, current - 1, 4, "ILLO", "ILLA", "ALLE")
                                )
                                || (
                                    (
                                        SubstringMatches(original, lastLength - 1, 2, "AS", "OS")
                                        || SubstringMatches(original, lastLength, 'A', 'O')
                                        )
                                    && SubstringMatches(original, current - 1, 4, "ALLE")
                                    )
                                )
                            {
                                primary += 'L';
                                // _secondary += "";
                                current += 2;
                                break;
                            }
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        primary += 'L';
                        secondary += 'L';
                        break;

                    case 'M':
                        if ((
                            SubstringMatches(original, current - 1, 3, "UMB")
                            && (
                                (current + 1) == lastLength
                                || SubstringMatches(original, current + 2, 2, "ER")
                                )
                            )
                            // "dumb", "thumb"
                            || original[current + 1] == 'M'
                            )
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        primary += 'M';
                        secondary += 'M';
                        break;

                    case 'N':
                        if (original[current + 1] == 'N')
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        primary += 'N';
                        secondary += 'N';
                        break;

                    case 'Ã': //Ã‡
                        ++current;
                        primary += 'N';
                        secondary += 'N';
                        break;

                    case 'P':
                        if (original[current + 1] == 'H')
                        {
                            current += 2;
                            primary += 'F';
                            secondary += 'F';
                            break;
                        }

                        // also account for "campbell" and "raspberry"
                        if (SubstringMatches(original, current + 1, 'P', 'B'))
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        primary += 'P';
                        secondary += 'P';
                        break;

                    case 'Q':
                        if (original[current + 1] == 'Q')
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        primary += 'K';
                        secondary += 'K';
                        break;

                    case 'R':
                        // french e.g. "rogier", but exclude "hochmeier"
                        if (current == lastLength
                            && !IsSlavoGermanic(original)
                            && SubstringMatches(original, current - 2, 2, "IE")
                            && !SubstringMatches(original, current - 4, 2, "ME", "MA")
                            )
                        {
                            // _primary   += "";
                            secondary += 'R';
                        }
                        else
                        {
                            primary += 'R';
                            secondary += 'R';
                        }
                        if (original[current + 1] == 'R')
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        break;

                    case 'S':
                        // special cases "island", "isle", "carlisle", "carlysle"
                        if (SubstringMatches(original, current - 1, 3, "ISL", "YSL"))
                        {
                            ++current;
                            break;
                        }

                        // special case "sugar-"
                        if (current == 0
                            && SubstringMatches(original, current, 5, "SUGAR")
                            )
                        {
                            primary += 'X';
                            secondary += 'S';
                            ++current;
                            break;
                        }

                        if (SubstringMatches(original, current, 2, "SH"))
                        {
                            // germanic
                            if (SubstringMatches(original, current + 1, 4, "HEIM", "HOEK", "HOLM", "HOLZ"))
                            {
                                primary += 'S';
                                secondary += 'S';
                            }
                            else
                            {
                                primary += 'X';
                                secondary += 'X';
                            }
                            current += 2;
                            break;
                        }

                        // italian & armenian 
                        if (SubstringMatches(original, current, 3, "SIO", "SIA")
                            || SubstringMatches(original, current, 4, "SIAN")
                            )
                        {
                            if (!IsSlavoGermanic(original))
                            {
                                primary += 'S';
                                secondary += 'X';
                            }
                            else
                            {
                                primary += 'S';
                                secondary += 'S';
                            }
                            current += 3;
                            break;
                        }

                        // german & anglicisations, e.g. "smith" match "schmidt", "snider" match "schneider"
                        // also, -sz- in slavic language altho in hungarian it is pronounced 's'
                        if ((
                            current == 0
                            && SubstringMatches(original, current + 1, 'M', 'N', 'L', 'W')
                            )
                            || SubstringMatches(original, current + 1, 'Z')
                            )
                        {
                            primary += 'S';
                            secondary += 'X';
                            if (SubstringMatches(original, current + 1, 'Z'))
                            {
                                current += 2;
                            }
                            else
                            {
                                ++current;
                            }
                            break;
                        }

                        if (SubstringMatches(original, current, 2, "SC"))
                        {
                            // Schlesinger"s rule 
                            if (original[current + 2] == 'H')
                                // dutch origin, e.g. "school", "schooner"
                                if (SubstringMatches(original, current + 3, 2, "OO", "ER", "EN", "UY", "ED", "EM"))
                                {
                                    // "schermerhorn", "schenker" 
                                    if (SubstringMatches(original, current + 3, 2, "ER", "EN"))
                                    {
                                        primary += 'X';
                                        secondary += "SK";
                                    }
                                    else
                                    {
                                        primary += "SK";
                                        secondary += "SK";
                                    }
                                    current += 3;
                                    break;
                                }
                                else
                                {
                                    if (current == 0
                                        && !IsVowel(original, 3)
                                        && original[current + 3] != 'W'
                                        )
                                    {
                                        primary += 'X';
                                        secondary += 'S';
                                    }
                                    else
                                    {
                                        primary += 'X';
                                        secondary += 'X';
                                    }
                                    current += 3;
                                    break;
                                }

                            if (SubstringMatches(original, current + 2, 'I', 'E', 'Y'))
                            {
                                primary += 'S';
                                secondary += 'S';
                                current += 3;
                                break;
                            }

                            // else
                            primary += "SK";
                            secondary += "SK";
                            current += 3;
                            break;
                        }

                        // french e.g. "resnais", "artois"
                        if (current == lastLength
                            && SubstringMatches(original, current - 2, 2, "AI", "OI")
                            )
                        {
                            // _primary   += "";
                            secondary += 'S';
                        }
                        else
                        {
                            primary += 'S';
                            secondary += 'S';
                        }

                        if (SubstringMatches(original, current + 1, 'S', 'Z'))
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        break;

                    case 'T':
                        if (SubstringMatches(original, current, 4, "TION"))
                        {
                            primary += 'X';
                            secondary += 'X';
                            current += 3;
                            break;
                        }

                        if (SubstringMatches(original, current, 3, "TIA", "TCH"))
                        {
                            primary += 'X';
                            secondary += 'X';
                            current += 3;
                            break;
                        }

                        if (SubstringMatches(original, current, 2, "TH")
                            || SubstringMatches(original, current, 3, "TTH")
                            )
                        {
                            // special case "thomas", "thames" or germanic
                            if (SubstringMatches(original, current + 2, 2, "OM", "AM")
                                || SubstringMatches(original, 0, 4, "VAN ", "VON ")
                                || SubstringMatches(original, 0, 3, "SCH")
                                )
                            {
                                primary += 'T';
                                secondary += 'T';
                            }
                            else
                            {
                                primary += '0';
                                secondary += 'T';
                            }
                            current += 2;
                            break;
                        }

                        if (SubstringMatches(original, current + 1, 'T', 'D'))
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        primary += 'T';
                        secondary += 'T';
                        break;

                    case 'V':
                        if (original[current + 1] == 'V')
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        primary += 'F';
                        secondary += 'F';
                        break;

                    case 'W':
                        // can also be in middle of word
                        if (SubstringMatches(original, current, 2, "WR"))
                        {
                            primary += 'R';
                            secondary += 'R';
                            current += 2;
                            break;
                        }

                        if ((current == 0)
                            && (
                                IsVowel(original, current + 1)
                                || SubstringMatches(original, current, 2, "WH")
                                )
                            )
                        {
                            // Wasserman should match Vasserman 
                            if (IsVowel(original, current + 1))
                            {
                                primary += 'A';
                                secondary += 'F';
                            }
                            else
                            {
                                // need Uomo to match Womo 
                                primary += 'A';
                                secondary += 'A';
                            }
                        }

                        // Arnow should match Arnoff
                        if ((
                            current == lastLength
                            && IsVowel(original, current - 1)
                            )
                            || SubstringMatches(original, current - 1, 5, "EWSKI", "EWSKY", "OWSKI", "OWSKY")
                            || SubstringMatches(original, 0, 3, "SCH")
                            )
                        {
                            // _primary   += "";
                            secondary += 'F';
                            ++current;
                            break;
                        }

                        // polish e.g. "filipowicz"
                        if (SubstringMatches(original, current, 4, "WICZ", "WITZ"))
                        {
                            primary += "TS";
                            secondary += "FX";
                            current += 4;
                            break;
                        }

                        // else skip it
                        ++current;
                        break;

                    case 'X':
                        // french e.g. breaux 
                        if (!(
                            current == lastLength
                            && (
                                SubstringMatches(original, current - 3, 3, "IAU", "EAU")
                                || SubstringMatches(original, current - 2, 2, "AU", "OU")
                                )
                            )
                            )
                        {
                            primary += "KS";
                            secondary += "KS";
                        }

                        if (SubstringMatches(original, current + 1, 'C', 'X'))
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        break;

                    case 'Z':
                        // chinese pinyin e.g. "zhao" 
                        if (original[current + 1] == 'H')
                        {
                            primary += 'J';
                            secondary += 'J';
                            current += 2;
                            break;
                        }
                        if (
                            SubstringMatches(original, current + 1, 2, "ZO", "ZI", "ZA")
                            || (
                                IsSlavoGermanic(original)
                                && (
                                    current > 0
                                    && original[current - 1] != 'T'
                                    )
                                )
                            )
                        {
                            primary += 'S';
                            secondary += "TS";
                        }
                        else
                        {
                            primary += 'S';
                            secondary += 'S';
                        }

                        if (original[current + 1] == 'Z')
                        {
                            current += 2;
                        }
                        else
                        {
                            ++current;
                        }
                        break;

                    default:
                        ++current;
                        break;
                } // end switch
            } // end while

            // printf("<br />ORIGINAL:   %s\n", _original);
            // printf("<br />current:    %s\n", _current);
            // printf("<br />PRIMARY:    %s\n", _primary);
            // printf("<br />SECONDARY:  %s\n", _secondary);

            primary = Substring(primary, 0, 4);
            secondary = Substring(secondary, 0, 4);

            if (primary == secondary)
            {
                secondary = null;
            }

            return (primary != null);
        }

        private static bool SubstringMatches(string text, int start, params char[] matchChars)
        {
            var ch = text[start];
            for (int i = 0, c = matchChars.Length; i < c; i++)
            {
                if (matchChars[i] == ch) return true;
            }
            return false;
        }

        private static bool SubstringMatches(string text, int start, int length, params string[] matchStrings)
        {
            if (start >= 0 && start < text.Length)
            {
                var str = Substring(text, start, length);
                for (int i = 0, c = matchStrings.Length; i < c; i++)
                {
                    if (matchStrings[i] == str) return true;
                }
            }

            return false;
        }

        private static bool IsVowel(string text, int pos)
        {
            return Regex.IsMatch(Substring(text, pos, 1), "[AEIOUY]");
        }

        private static bool IsSlavoGermanic(string text)
        {
            return Regex.IsMatch(text, "W|K|CZ|WITZ");
        }

        private static string Substring(string text, int pos, int length)
        {
            return text.Substring(pos, Math.Min(length, text.Length));
        }

        public class Result
        {
            private readonly string primary;
            private readonly string secondary;

            public Result(string primary, string secondary)
            {
                this.primary = primary;
                this.secondary = secondary;
            }

            public string Primary
            {
                get
                {
                    return primary;
                }
            }

            public string Secondary
            {
                get
                {
                    return secondary;
                }
            }
        }
    }
}