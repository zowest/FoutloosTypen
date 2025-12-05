using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Maui.Storage;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;
using System.Diagnostics;

namespace FoutloosTypen.Core.Data.Repositories
{
    public class PracticeMaterialRepository : DatabaseConnection, IPracticeMaterialRepository
    {
        public PracticeMaterialRepository()
        {
            try
            {
                CreateTable(@"
                    CREATE TABLE IF NOT EXISTS PracticeMaterials (
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        Sentence NVARCHAR(500) NOT NULL,
                        AssignmentId INTEGER NOT NULL
                    );
                ");

                InsertDefaultPracticeMaterials();
                Debug.WriteLine("PracticeMaterialRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PracticeMaterialRepository initialization error: {ex.Message}");
                throw;
            }
        }

        private void InsertDefaultPracticeMaterials()
        {
            List<string> insertQueries = new();

            // CURSUS 1 - BEGINNERS (~120 karakters per les = 20 seconden bij 60 WPM)
            
            // Les 1
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De kat speelt met een bal. De hond rent door de tuin. Het is een mooie zonnige dag vandaag. Ik zie een vogel in de boom zitten.', 1)");
            
            // Les 2
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Mijn vriend heet Jan en woont in Amsterdam. We gaan vaak samen wandelen in het park. Het weer is vandaag erg mooi en warm buiten.', 2)");
            
            // Les 3
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Hoe laat is het nu? Waar woon jij precies? Wat ga je vandaag doen? Kun je mij helpen met deze taak? Wanneer kom je bij mij langs?', 3)");
            
            // Les 4
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De winkel is open van negen tot zes uur. Kom gerust binnen om rond te kijken. We hebben veel nieuwe producten deze week in de aanbieding.', 4)");
            
            // Les 5
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Pak je tas in voor school. Doe de deur achter je dicht. Vergeet niet je sleutel mee te nemen. Zet de lamp uit als je weggaat vandaag.', 5)");
            
            // Les 6
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Goedemorgen allemaal en welkom! Fijne dag gewenst vandaag. Tot ziens en tot de volgende keer. Bedankt voor je hulp en je tijd hierbij.', 6)");
            
            // Les 7
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het is nu precies tien uur in de ochtend. Ik heb vijf rode appels gekocht bij de winkel. Morgen is het dinsdag 15 maart om half drie.', 7)");
            
            // Les 8
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Hoeveel kost deze mooie blauwe jas? Ik wil graag met de pinpas betalen. Heeft u ook schoenen in maat 42 op voorraad in deze winkel?', 8)");
            
            // Les 9
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het regent hard vandaag dus neem een paraplu mee. De lente is het mooiste seizoen van het jaar. Morgen wordt het waarschijnlijk zonnig weer.', 9)");
            
            // Les 10 (Toets)
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Ik ben helemaal klaar met de oefening. Deze test was niet zo moeilijk als ik dacht. Goed gedaan en ga zo door met oefenen elke dag verder!', 10)");

            // CURSUS 2 - GEVORDERDEN (~180 karakters per les = 30 seconden bij 60 WPM)
            
            // Les 1
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het is belangrijk om gezond te eten en voldoende te bewegen. Vandaag ga ik naar de markt om verse groenten en fruit te kopen voor het hele gezin. Sport helpt je om fit en gezond te blijven gedurende je leven.', 11)");
            
            // Les 2
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Beste mevrouw Jansen, hartelijk dank voor uw snelle reactie op mijn bericht. We zullen uw verzoek met de hoogste prioriteit in behandeling nemen deze week. Met vriendelijke groet namens het hele team van ons bedrijf.', 12)");
            
            // Les 3
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Er was eens een mooie prinses die in een groot kasteel woonde samen met haar familie. Op een dag besloot ze om op avontuur te gaan naar verre landen. Ze ontdekte een magisch bos achter de hoge bergen waar niemand ooit was geweest.', 13)");
            
            // Les 4
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De computer bestaat uit verschillende onderdelen zoals de processor, het werkgeheugen en de harde schijf. Het moederbord verbindt alle componenten met elkaar zodat ze kunnen samenwerken. De voeding zorgt voor de nodige stroom naar alle delen.', 14)");
            
            // Les 5
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Volgens de laatste wetenschappelijke berichten stijgt de gemiddelde temperatuur wereldwijd elk jaar verder. Wetenschappers waarschuwen nadrukkelijk voor de ernstige gevolgen van klimaatverandering. Duurzame energie wordt steeds belangrijker voor onze toekomst.', 15)");
            
            // Les 6
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Hoewel het buiten erg koud en guur was, gingen we toch een lange wandeling maken in het bos. De film die we gisteren avond hebben gezien was echt fantastisch en indrukwekkend. Ik zou het iedereen willen aanraden om deze te bekijken.', 16)");
            
            // Les 7
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Goedemiddag meneer, kan ik u ergens mee helpen vandaag in onze winkel? Ja graag, ik zoek informatie over de nieuwe voorjaarscollectie die net binnen is gekomen. Die vindt u op de tweede verdieping helemaal achterin de zaak bij het raam.', 17)");
            
            // Les 8
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Geachte heer de Vries, hierbij bevestig ik graag de ontvangst van uw schriftelijke aanvraag van afgelopen week. We zullen u binnen tien werkdagen van een uitgebreide reactie voorzien per email. Alvast hartelijk dank voor uw geduld en begrip hierin.', 18)");
            
            // Les 9
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Dit prachtige restaurant biedt een uitstekende service en heerlijk eten tegen redelijke prijzen elke dag. De sfeer is bijzonder gezellig en warm met mooie decoratie en zachte muziek op de achtergrond. Ik kan het werkelijk iedereen van harte aanraden!', 19)");
            
            // Les 10 (Toets)
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Deze uitgebreide oefening test grondig je typsnelheid en nauwkeurigheid onder druk van de tijd. Probeer tijdens het typen zo weinig mogelijk fouten te maken en blijf gefocust. Veel succes met deze belangrijke toets voor gevorderden!', 20)");

            // CURSUS 3 - EXPERT (~240 karakters per les = 40 seconden bij 60 WPM)
            
            // Les 1
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('In de schemerige schemering van de late herfstavond wandelde de eenzame protagonist nadenkend door de verlaten straten van de oude binnenstad. De melancholische gedachten aan vervlogen tijden weerspiegelden zich duidelijk in de weemoed van zijn gepeinzende blik. Het zachte gefluister van de wind leek de echo''s van het verleden met zich mee te voeren door de lege straten.', 21)");
            
            // Les 2
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Recente wetenschappelijke onderzoeken tonen overtuigend aan dat de correlatie tussen klimaatverandering en menselijke activiteit onmiskenbaar is geworden. De complexe interactie tussen atmosferische condities en oceaanstromingen vereist zonder twijfel een interdisciplinaire benadering. Methodologische overwegingen spelen een cruciale rol in de validiteit van alle empirische bevindingen.', 22)");
            
            // Les 3
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Ingevolge artikel 6:162 van het Burgerlijk Wetboek is degene die jegens een ander een onrechtmatige daad pleegt volledig aansprakelijk. Het causale verband tussen de gedraging en de ontstane schade dient adequaat te worden vastgesteld conform de geldende rechtspraak. Voornoemde wettelijke bepalingen zijn van dwingend recht en kunnen niet contractueel worden uitgesloten door partijen.', 23)");
            
            // Les 4
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De fundamentele vraag naar de essentie van het zijn heeft filosofen door de eeuwen heen intensief beziggehouden met existentiële contemplatie en diepe reflectie. Kant''s transcendentale idealisme postuleert helder dat onze kennis beperkt is tot de wereld zoals die aan ons verschijnt. De dialectische spanning tussen rationalisme en empirisme kenmerkt de epistemologische discours van de moderne filosofie volledig.', 24)");
            
            // Les 5
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Bij de installatie van het softwarepakket dient u ervoor te zorgen dat alle systeemvereisten nauwkeurig worden voldaan voorafgaand aan de implementatie. De configuratie van de netwerkparameters vereist administratieve privileges en een grondige kennis van TCP/IP-protocollen en netwerkarchitectuur. Raadpleeg de troubleshooting guide indien u foutmeldingen ontvangt tijdens het installatieproces van deze software.', 25)");
            
            // Les 6
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('In het heldere maanlicht dansten schaduwen mysterieus, het gefluister van de stille nacht, de stilte spreekt welsprekend tot de ziel. Bloemblaadjes dwarrelen langzaam neer als confetti van voorbije dromen en vergeten beloften uit het verleden. De rivier van tijd stroomt onverbiddelijk voort, meeslepend alles wat ooit betekenis had in ons bestaan door de geschiedenis heen.', 26)");
            
            // Les 7
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Tijdens de bloeiperiode van de Renaissance onderging Europa een ongekende culturele en intellectuele transformatie die de fundamenten legde voor de moderne beschaving. De herontdekking van klassieke teksten stimuleerde een hernieuwde belangstelling voor humanistische waarden en wetenschappelijke inquiry bij geleerden. Prominente figuren zoals Leonardo da Vinci en Michelangelo belichaamden perfect het ideaal van de Renaissance-mens.', 27)");
            
            // Les 8
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De premisse dat technologische vooruitgang onvoorwaardelijk leidt tot maatschappelijke verbetering is een discutabele aanname die kritisch onderzocht dient te worden. Enerzijds faciliteren innovaties ongekende mogelijkheden voor communicatie en efficiëntie, anderzijds genereren zij nieuwe vormen van sociale fragmentatie. Een genuanceerde analyse vereist het zorgvuldig afwegen van zowel de potentiële voordelen als de inherente risico''s.', 28)");
            
            // Les 9
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De protagonist bevond zich diep in een existentiële crisis, verscheurd tussen de verlangens van het hart en de strenge dictaten van de rede. In de surrealistische setting van het verhaal verweven zich dromen en werkelijkheid tot een caleidoscopisch geheel van betekenislagen. De auteur hanteert vakkundig een stream-of-consciousness techniek om de innerlijke turbulentie van het personage te verbeelden.', 29)");
            
            // Les 10 (Toets)
            insertQueries.Add(@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Deze afsluitende toets evalueert grondig uw volledige beheersing van complexe tekststructuren en uw vermogen tot nauwkeurige en efficiënte tekstverwerking onder tijdsdruk. Demonstreer uw expertise door foutloos te typen met behoud van een hoge snelheid gedurende de gehele opdracht zonder fouten. Het succesvolle afronden van dit niveau getuigt van uitzonderlijke typvaardigheid enkunde op het hoogste niveau.', 30)");

            InsertMultipleWithTransaction(insertQueries);
            Debug.WriteLine($"Inserted {insertQueries.Count} practice materials");
        }

        public List<PracticeMaterial> GetAll()
        {
            List<PracticeMaterial> practiceMaterials = new();

            try
            {
                OpenConnection();

                using var command = Connection.CreateCommand();
                command.CommandText = "SELECT Id, Sentence, AssignmentId FROM PracticeMaterials";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string sentence = reader.GetString(1);
                    int assignmentId = reader.GetInt32(2);

                    practiceMaterials.Add(new PracticeMaterial(id, sentence, assignmentId));
                }

                Debug.WriteLine($"Retrieved {practiceMaterials.Count} practice materials");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving practice materials: {ex.Message}");
                throw;
            }
            finally
            {
                CloseConnection();
            }

            return practiceMaterials;
        }

        public PracticeMaterial? Get(int id)
        {
            PracticeMaterial? practiceMaterial = null;

            try
            {
                string query = "SELECT Id, Sentence, AssignmentId FROM PracticeMaterials WHERE Id = @Id";

                OpenConnection();
                using (SqliteCommand command = new(query, Connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    SqliteDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string sentence = reader.GetString(1);
                        int assignmentId = reader.GetInt32(2);

                        practiceMaterial = new PracticeMaterial(id, sentence, assignmentId);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving practice material with Id {id}: {ex.Message}");
                throw;
            }
            finally
            {
                CloseConnection();
            }

            return practiceMaterial;
        }
    }
}
