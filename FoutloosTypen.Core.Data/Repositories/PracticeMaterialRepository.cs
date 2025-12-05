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
            int assignmentId = 1;

            // CURSUS 1 - BEGINNERS (~120 karakters per zin = 20 seconden bij 60 WPM)
            // 10 lessen x 5 opdrachten = 50 zinnen
            
            // Les 1 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De kat speelt met een bal. De hond rent door de tuin. Het is een mooie zonnige dag vandaag. Ik zie een vogel in de boom zitten.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het huis heeft een rode deur. De tuin is vol met bloemen. Kinderen spelen buiten op straat. De zon schijnt helder aan de hemel.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Mijn auto staat voor het huis. De fiets is nieuw en blauw. We gaan wandelen in het park. Het weer is vandaag erg mooi weer.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De school begint om acht uur. Leerlingen lopen naar binnen. De leraar staat voor het bord. Iedereen is klaar voor de les.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het boek ligt op tafel. De lamp geeft veel licht. Ik lees graag in de avond. Het is stil en rustig hier.', {assignmentId++})");
            
            // Les 2 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Mijn vriend heet Jan en woont in Amsterdam. We gaan vaak samen wandelen in het park. Het weer is vandaag erg mooi en warm buiten.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De leraar schrijft op het bord. Leerlingen maken aantekeningen. Het is stil in de klas nu. Iedereen luistert goed naar de uitleg.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De winkel verkoopt verse groente. Ik koop appels en peren. De kassière is vriendelijk. Het brood ruikt lekker vandaag hier.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De bus komt om half negen. We stappen in bij het station. Het is druk in de ochtend. Veel mensen gaan naar hun werk.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Mijn zus studeert medicijnen. Ze werkt hard elke dag. De universiteit is ver weg. Maar ze vindt het heel leuk.', {assignmentId++})");
            
            // Les 3 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Hoe laat is het nu? Waar woon jij precies? Wat ga je vandaag doen? Kun je mij helpen met deze taak? Wanneer kom je bij mij langs?', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Wie is die man daar? Waarom ben je hier? Welke kleur vind je mooi? Hoeveel kost dit shirt? Mag ik even kijken hier?', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Ga je mee naar huis? Heb je tijd voor mij? Wil je koffie drinken? Kun je dat herhalen? Begrijp je wat ik bedoel?', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Waar is het station? Hoe kom ik daar precies? Wat is de beste route? Kun je de weg uitleggen? Is het ver lopen hiervandaan?', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Hoe gaat het met jou? Wat heb je gedaan vandaag? Ben je moe van werken? Wil je even pauzeren? Zullen we iets eten straks?', {assignmentId++})");
            
            // Les 4 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De winkel is open van negen tot zes uur. Kom gerust binnen om rond te kijken. We hebben veel nieuwe producten deze week in de aanbieding.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Let op de gladde vloer hier. Loop voorzichtig door de gang. De uitgang is rechts om de hoek. Bedankt voor uw bezoek vandaag.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De vergadering start om twee uur. Neem uw documenten mee. De koffie staat klaar beneden. We beginnen met de agenda nu.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het restaurant sluit om elf uur. Reserveren is aan te raden. De menukaart is vernieuwd. Wij wensen u smakelijk eten.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De bibliotheek is nu open. Stilte wordt gewaardeerd hier. Boeken moet u zelf terugbrengen. De computers zijn vrij beschikbaar.', {assignmentId++})");
            
            // Les 5 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Pak je tas in voor school. Doe de deur achter je dicht. Vergeet niet je sleutel mee te nemen. Zet de lamp uit als je weggaat vandaag.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Was je handen voor het eten. Zet de televisie uit nu. Ruim je kamer op alsjeblieft. Ga op tijd naar bed vanavond.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Trek je jas aan buiten. Neem een paraplu mee vandaag. Wacht op mij bij de bushalte. Vergeet je huiswerk niet mee te nemen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Maak eerst je huiswerk af. Eet gezond tijdens de lunch. Beweeg voldoende elke dag. Drink genoeg water tussen door.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Check je email regelmatig. Beantwoord berichten snel graag. Bewaar belangrijke documenten goed. Maak een backup van bestanden.', {assignmentId++})");
            
            // Les 6 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Goedemorgen allemaal en welkom! Fijne dag gewenst vandaag. Tot ziens en tot de volgende keer. Bedankt voor je hulp en je tijd hierbij.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Hallo daar, hoe gaat het? Prettige avond toegewenst verder. We spreken elkaar snel weer. Pas goed op jezelf vandaag.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Goedemiddag meneer de Vries hier. Aangenaam kennis te maken. Het was leuk u te ontmoeten. Ik hoor graag van u.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Welkom in ons mooie huis. Ga lekker zitten op de bank. Wil je iets te drinken misschien? Voel je helemaal thuis hier.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Goedenavond beste mensen hier. Leuk dat je er bent vanavond. We gaan beginnen met het programma. Veel plezier allemaal gewenst vandaag.', {assignmentId++})");
            
            // Les 7 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het is nu precies tien uur in de ochtend. Ik heb vijf rode appels gekocht bij de winkel. Morgen is het dinsdag 15 maart om half drie.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('We vertrekken om kwart over acht. De trein komt aan om tien voor negen. Het duurt ongeveer 45 minuten reizen. We zijn er om half tien.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Ik heb 3 boeken gelezen vorige week. Het kostme 20 euro per stuk totaal. Er zijn 365 dagen in een jaar. Vandaag is de 12e dag.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De temperatuur is 18 graden nu. Het regent 5 millimeter per uur. Wind komt uit het zuiden nu. Het is 60 procent bewolkt.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De afstand is 25 kilometer ver. We rijden 80 kilometer per uur. Het duurt 30 minuten ongeveer. We tanken voor 50 euro.', {assignmentId++})");
            
            // Les 8 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Hoeveel kost deze mooie blauwe jas? Ik wil graag met de pinpas betalen. Heeft u ook schoenen in maat 42 op voorraad in deze winkel?', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Mag ik deze broek passen alsjeblieft? Waar is de paskamer precies? De prijs is iets te hoog vind ik. Is er misschien korting mogelijk?', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Ik zoek een cadeau voor mijn moeder. Heeft u iets moois onder de vijftig? Dit ziet er goed uit denk ik. Kunt u het inpakken graag?', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Hebben jullie verse groenten vandaag? Wat is de aanbieding deze week? Ik neem twee kilo aardappelen mee. Doe er drie uien bij.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Waar kan ik brood vinden hier? Is het vers gebakken vandaag? Ik neem een heel brood graag. Heeft u ook croissants beschikbaar?', {assignmentId++})");
            
            // Les 9 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het regent hard vandaag dus neem een paraplu mee. De lente is het mooiste seizoen van het jaar. Morgen wordt het waarschijnlijk zonnig weer.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De herfst brengt veel wind en regen. De bladeren vallen van de bomen nu. Het wordt steeds kouder buiten. Winter komt er snel aan.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De zomer is warm en droog geweest. We gingen vaak zwemmen in zee. De temperatuur was boven de dertig. Het was perfect vakantieweer toen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Sneeuw valt zacht op de grond neer. Kinderen maken een grote sneeuwpop. Het vriest vannacht stevig door. Schaatsen kan morgen op het ijs.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De zon komt op om zes uur. Vogels fluiten vrolijk in de bomen. De dauw ligt op het gras nu. Het wordt een prachtige dag vandaag.', {assignmentId++})");
            
            // Les 10 (Toets) - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Ik ben helemaal klaar met de oefening. Deze test was niet zo moeilijk als ik dacht. Goed gedaan en ga zo door met oefenen elke dag verder!', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De toets ging beter dan verwacht vandaag. Ik heb mijn best gedaan op alle vragen. Nu kan ik verder naar het volgende niveau toe.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Alle opdrachten zijn nu af gemaakt. Het typen gaat steeds beter nu. Ik ben trots op mijn vooruitgang hier. Op naar de volgende uitdaging!', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De basisvaardigheden zijn nu geleerd goed. Ik typ steeds sneller en nauwkeuriger. Het oefenen heeft zeker geholpen veel. Klaar voor de volgende stap.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het eerste niveau is succesvol afgerond. Mijn typsnelheid is verbeterd flink. Ik maak minder fouten dan voorheen. Tijd voor grotere uitdagingen nu.', {assignmentId++})");

            // CURSUS 2 - GEVORDERDEN (~180 karakters per zin = 30 seconden bij 60 WPM)
            // 10 lessen x 5 opdrachten = 50 zinnen
            
            // Les 1 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het is belangrijk om gezond te eten en voldoende te bewegen. Vandaag ga ik naar de markt om verse groenten en fruit te kopen voor het hele gezin. Sport helpt je om fit en gezond te blijven gedurende je leven.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Een gebalanceerd dieet bevat groenten, fruit en eiwitten dagelijks. Water drinken is essentieel voor een goede gezondheid altijd. Vermijd te veel suiker en bewerkte voeding in je eten. Kies voor natuurlijke producten waar mogelijk steeds.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Regelmatig sporten verbetert je conditie en uithoudingsvermogen significant. Een wandeling van dertig minuten per dag is al voldoende voor beginners. Luister goed naar je lichaam en rust wanneer dat nodig is altijd. Gezondheid is je belangrijkste bezit in het leven nu.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Voldoende slaap is cruciaal voor herstel en concentratie overdag steeds. Probeer elke nacht minstens zeven uur te slapen voor optimaal resultaat. Vermijd scherpe schermen vlak voor het slapengaan in de avond. Een goede nachtrust bepaalt hoe je dag verloopt morgen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Stress kan leiden tot verschillende gezondheidsproblemen op lange termijn helaas. Ontspanning en mindfulness helpen om stress te verminderen effectief dagelijks. Neem tijd voor jezelf en doe dingen die je gelukkig maken altijd. Je mentale gezondheid is net zo belangrijk als fysieke gezondheid nu.', {assignmentId++})");

            // Les 2 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Beste mevrouw Jansen, hartelijk dank voor uw snelle reactie op mijn bericht. We zullen uw verzoek met de hoogste prioriteit in behandeling nemen deze week. Met vriendelijke groet namens het hele team van ons bedrijf.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Uw aanvraag is in goede orde ontvangen en wordt binnenkort behandeld door onze specialisten. Wij streven ernaar om binnen vijf werkdagen contact met u op te nemen met een update hierover. Dank u voor uw geduld en begrip in deze zaak.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Gelieve ons te excuseren voor het ongemak veroorzaakt door deze kwestie. Wij doen ons uiterste best om dit zo snel mogelijk op te lossen voor u. Uw tevredenheid is van het grootste belang voor ons bedrijf altijd.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Wij waarderen uw feedback zeer en zullen deze zeker in overweging nemen voor toekomstige verbeteringen. Mocht u nog verdere vragen of opmerkingen hebben, aarzel dan niet om contact met ons op te nemen. Wij staan altijd voor u klaar om te helpen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het was ons een genoegen om u van dienst te zijn onlangs. Wij hopen dat alles naar wens is opgelost voor u inmiddels. Nogmaals onze oprechte excuses voor het ontstane ongemak en dank voor uw begrip.', {assignmentId++})");

            // Les 3 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Er was eens een mooie prinses die in een groot kasteel woonde samen met haar familie. Op een dag besloot ze om op avontuur te gaan naar verre landen. Ze ontdekte een magisch bos achter de hoge bergen waar niemand ooit was geweest.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Diep in het hart van het koninkrijk lag een verborgen tuin vol met eeuwige bloemen. Geen enkele voetstap had ooit de grond ertussen verstoord, tot op de dag dat zij kwam. De bloemen openden zich langzaam bij haar komst, als een welkom thuis na jaren van zoeken.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Vandaag de dag in een drukke stad, onze heldin Julia, een onbekende schrijfster, worstelt met haar nieuwste roman. Haar imaginatie lijkt opgedroogd temidden van de stadskaos. Maar zelfs hier, in deze overweldigende omgeving, fluisteren verhalen om haar heen, wachtend om ontdekt te worden door een attente ziel.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het oude morele verhaal van de tortoise en de hare herinnert ons eraan dat langzaam maar gestaag de race wint. Dit fabelachtige verhaal is door de jaren heen doorgegeven als een wijze les in volharding en toewijding. Laat het ons steeds bij blijven als een leidraad in ons eigen leven.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Een dappere ridder, gezworen om de zwakken te beschermen, trekt ten strijde tegen een verschrikkelijke draak die het koninkrijk terroriseert. Gewapend met slechts een zwaard en een schild, confronteert hij het beest in een epische strijd om de vrijheid van zijn volk.', {assignmentId++})");
            
            // Les 4 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De computer bestaat uit verschillende onderdelen zoals de processor, het werkgeheugen en de harde schijf. Het moederbord verbindt alle componenten met elkaar zodat ze kunnen samenwerken. De voeding zorgt voor de nodige stroom naar alle delen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Elke app op uw telefoon is ontworpen met de gebruiker in gedachten, met eenvoudige navigatie en nuttige functies. Updates worden regelmatig uitgebracht om de prestaties en veiligheid te verbeteren. Het is essentieel om deze updates tijdig te installeren voor een optimale ervaring.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het besturingssysteem op uw computer beheert alle hardware en software bronnen. Het stelt u in staat om programma''s uit te voeren en bestanden te beheren. Regelmatige updates zorgen ervoor dat uw systeem veilig en efficiënt blijft functioneren.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Wist je dat de eerste computervirus in 1986 werd ontdekt en zich verspreidde via een floppy disk? Tegenwoordig verspreiden virussen zich via internet en kunnen ze ernstige schade toebrengen aan je apparaten. Een goede antivirussoftware is essentieel om je systemen te beschermen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het internet is een wereldwijd netwerk van verbonden computers die informatie delen. Het maakt gebruik van gestandaardiseerde communicatieprotocollen om gegevensoverdracht mogelijk te maken. Dankzij het internet kunnen we in real-time communiceren en overal ter wereld toegang krijgen tot informatie.', {assignmentId++})");
            
            // Les 5 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Volgens de laatste wetenschappelijke berichten stijgt de gemiddelde temperatuur wereldwijd elk jaar verder. Wetenschappers waarschuwen nadrukkelijk voor de ernstige gevolgen van klimaatverandering. Duurzame energie wordt steeds belangrijker voor onze toekomst.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De mensheid staat voor grote uitdagingen door de snelle klimaatverandering. Het is van vitaal belang om nu actie te ondernemen om verdere schade aan onze planeet te voorkomen. Elk individu kan bijdragen aan een duurzamere wereld door bewuste keuzes te maken in hun dagelijks leven.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Wetenschappers adviseren dringend om de uitstoot van broeikasgassen wereldwijd te verminderen. Dit kan door over te schakelen op hernieuwbare energiebronnen zoals zon, wind en water. Ook het verbeteren van de energie-efficiëntie speelt een cruciale rol in het bestrijden van klimaatverandering.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het bevorderen van duurzame stedelijke ontwikkeling is essentieel in de strijd tegen klimaatverandering. Steden zijn verantwoordelijk voor een groot deel van de wereldwijde CO2-uitstoot. Slimme steden die gebruik maken van innovatieve technologieën kunnen aanzienlijk bijdragen aan het verminderen van deze uitstoot.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Internationale samenwerking is cruciaal om de klimaatdoelen te behalen. Landen moeten hun krachten bundelen en samen investeren in duurzame oplossingen. Alleen dan kunnen we de ergste gevolgen van klimaatverandering voorkomen en een leefbare toekomst garanderen voor komende generaties.', {assignmentId++})");
            
            // Les 6 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Hoewel het buiten erg koud en guur was, gingen we toch een lange wandeling maken in het bos. De film die we gisteren avond hebben gezien was echt fantastisch en indrukwekkend. Ik zou het iedereen willen aanraden om deze te bekijken.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Ondanks de regenachtige voorspellingen besloten we om ons niet te laten tegenhouden en gingen we toch fietsen. Het was een verfrissende ervaring om door de natuur te rijden met alle geurige bloemen in bloei. We kwamen zelfs een paar vriendelijke dieren tegen langs de weg.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het boek dat ik laatst heb gelezen, opent met een krachtige zin die meteen je aandacht trekt. Bladzijden vlogen om terwijl ik volledig in het verhaal werd getrokken. De unieke schrijfwijze van de auteur maakt het lezen ervan een waar genot.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Dit schilderij, geschilderd door een onbekende meester in de 18e eeuw, toont een prachtig landschap met levendige kleuren. Het verhaal gaat dat het schilderij aan deze muur hing in een kasteel in Frankrijk, voordat het hierheen werd verplaatst.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Tijdens onze laatste vakantie bezochten we een klein dorpje dat beroemd is om zijn traditionele ambachten. Het was fascinerend om te zien hoe de lokale kunstenaars hun goederen maakten met zulke toewijding en vaardigheid. Uiteraard hebben we een aantal unieke souvenirs gekocht als herinnering.', {assignmentId++})");
            
            // Les 7 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Goedemiddag meneer, kan ik u ergens mee helpen vandaag in onze winkel? Ja graag, ik zoek informatie over de nieuwe voorjaarscollectie die net binnen is gekomen. Die vindt u op de tweede verdieping helemaal achterin de zaak bij het raam.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Misschien kunnen we het raam openzetten voor wat frisse lucht? Het is belangrijk om tijdens het werken goed te ventileren. Te veel stilstaande lucht kan ongezond zijn op de lange termijn.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Zou u zo vriendelijk willen zijn om het licht uit te doen als u deze kamer verlaat? Het helpt om energie te besparen en is beter voor het milieu. Alvast bedankt voor uw medewerking.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Ik waardeer uw geduld terwijl ik deze informatie voor u opzoek. Het is altijd ons doel om de best mogelijke service te bieden aan onze klanten. Voelt u zich vrij om andere vragen te stellen terwijl u wacht.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Bij voorbaat dank voor het invullen van onze vragenlijst. Uw feedback is van onschatbare waarde voor ons en helpt ons onze producten en diensten te verbeteren. We beloven het snel en efficiënt te behandelen.', {assignmentId++})");
            
            // Les 8 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Geachte heer de Vries, hierbij bevestig ik graag de ontvangst van uw schriftelijke aanvraag van afgelopen week. We zullen u binnen tien werkdagen van een uitgebreide reactie voorzien per email. Alvast hartelijk dank voor uw geduld en begrip hierin.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Hartelijk dank voor uw aanvraag. Wij hebben deze in goede orde ontvangen en zullen deze z.s.m. behandelen. U ontvangt snel van ons een bevestiging per email met verdere informatie.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Wij bevestigen hiermee de afspraak die wij met u gemaakt hebben voor een telefonisch overleg. Onze medewerker zal u bellen op het afgesproken tijdstip om uw vragen te bespreken.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Graag zouden wij uw aandacht willen vragen voor ons nieuwe product dat binnenkort gelanceerd wordt. U ontvangt hiervoor een persoonlijke uitnodiging voor onze online presentatie.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Dit is een standaard bevestiging dat wij uw verzoek om informatie over ons product hebben ontvangen. Een van onze vertegenwoordigers zal zo spoedig mogelijk contact met u opnemen.', {assignmentId++})");
            
            // Les 9 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Dit prachtige restaurant biedt een uitstekende service en heerlijk eten tegen redelijke prijzen elke dag. De sfeer is bijzonder gezellig en warm met mooie decoratie en zachte muziek op de achtergrond. Ik kan het werkelijk iedereen van harte aanraden!', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De combinatie van smaken in dit gerecht is werkelijk subliem. Het gebruik van verse, lokale ingredienten maakt een enorm verschil. Elk hapje is een smaakexplosie die je meeneemt op een culinaire reis.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Onze chef-kok heeft jarenlange ervaring en heeft voor toprestaurants over de hele wereld gewerkt. Zijn passie voor koken is duidelijk merkbaar in elk gerecht dat hij bereidt. Een echte aanrader voor fijnproevers!', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Deze wijn past perfect bij het voorgerecht dat je hebt gekozen. De subtiele tonen van eikenhout en vanille completeren de smaken van het gerecht. Een perfecte combinatie voor een geweldige avond.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Wij bieden een breed scala aan vegetarische en veganistische opties aan op ons menu. Onze gerechten zijn niet alleen lekker, maar ook gezond en gezond voor de planeet. Vraag gerust onze ober naar de mogelijkheden.', {assignmentId++})");
            
            // Les 10 (Toets) - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Deze uitgebreide oefening test grondig je typsnelheid en nauwkeurigheid onder druk van de tijd. Probeer tijdens het typen zo weinig mogelijk fouten te maken en blijf gefocust. Veel succes met deze belangrijke toets voor gevorderden!', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Bereid je goed voor op deze toets door dagelijks te oefenen. Probeer verschillende teksten en zinnen om je vaardigheden te verbeteren. Vergeet niet pauzes te nemen om vermoeidheid te voorkomen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Tijdens deze toets is het belangrijk om je aandacht bij het typen te houden. Laat je niet afleiden door omgevingsgeluiden of andere verstoringen. Concentreer je op het scherm en de te typen tekst.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Na het indienen van je toets, ontvang je meteen je resultaten en feedback. Gebruik deze informatie om te zien waar je sterke punten liggen en op welke gebieden je nog kunt verbeteren.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Onthoud dat deze toets een kans is om je vaardigheden te laten zien en te testen wat je tot nu toe hebt geleerd. Blijf rustig, adem diep in en uit, en begin dan met vertrouwen te typen.', {assignmentId++})");

            // CURSUS 3 - EXPERT (~240 karakters per zin = 40 seconden bij 60 WPM)
            // 10 lessen x 5 opdrachten = 50 zinnen
            
            // Les 1 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('In de schemerige schemering van de late herfstavond wandelde de eenzame protagonist nadenkend door de verlaten straten van de oude binnenstad. De melancholische gedachten aan vervlogen tijden weerspiegelden zich duidelijk in de weemoed van zijn gepeinzende blik. Het zachte gefluister van de wind leek de echo''s van het verleden met zich mee te voeren door de lege straten.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De nachtelijke stilte werd slechts doorbroken door het regelmatige getik van zijn schoenen op de klinkers onder zijn voeten steeds. Straatlantaarns wierpen lange schaduwen die dansten op de oude gevels van de huizen rondom. Hij voelde zich verbonden met de geschiedenis die in elke steen verankerd leek te zijn diep. De stad ademde herinneringen uit elke hoek en gat nu.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Geduldig wachtte de oude wijze man, gezeten op een verweerde koffer, op de komst van de volgende trein. Zijn unifoormantel, versleten maar schoon, flapte in de koude wind. In zijn handeen houten hengel, niet voor de vissen, maar als steun in zijn dagelijkse overpeinzingen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Vol trots toonde de jonge kunstenaar zijn nieuwste creatie, een meesterwerk van kleur en emotie, aan de wereld. Elke streek van zijn penseel vertelde een verhaal, zijn passie en toewijding duidelijk zichtbaar in elk detail. Dit was meer dan zomaar een schilderij; het was een venster naar zijn ziel.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De oude bibliotheek rook naar meerdere levens die tussen de pagina''s waren verloren. Elk boek, een wereld op zich, wachtend om herontdekt te worden door een nieuw paar ogen. Stilletjes fluisterden ze hun verhalen, verlangend naar de aanraking van een lezer.', {assignmentId++})");
            
            // Les 2 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Recente wetenschappelijke onderzoeken tonen overtuigend aan dat de correlatie tussen klimaatverandering en menselijke activiteit onmiskenbaar is geworden. De complexe interactie tussen atmosferische condities en oceaanstromingen vereist zonder twijfel een interdisciplinaire benadering. Methodologische overwegingen spelen een cruciale rol in de validiteit van alle empirische bevindingen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Uit recent gepubliceerd onderzoek blijkt dat de stijging van de zeespiegel versnelt door het smelten van gletsjers en ijskappen. Dit vormt een directe bedreiging voor laaggelegen kustgebieden wereldwijd. Er zijn dringende maatregelen nodig om de ergste gevolgen van deze ontwikkeling te beperken.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De bevindingen van deze studie benadrukken de noodzaak voor meer onderzoek naar duurzame energiebronnen. Slechts een haastige overgang naar duurzame praktijken kan de huidige achteruitgang van ons milieu stoppen. Alle opties moeten worden overwogen en substantiële investeringen gedaan worden in innovatieve technologieën.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het gebruik van fossiele brandstoffen moet onmiddellijk verminderd worden om de uitstoot van broeikasgassen te beperken. Hernieuwbare energiebronnen zoals zon en wind moeten op grote schaal geïmplementeerd worden. Daarnaast is het essentieel om energiebesparing en efficiency in alle sectoren te bevorderen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Individuen kunnen ook bijdragen door hun ecologische voetafdruk te verkleinen. Dit kan door minder vlees te eten, openbaar vervoer te gebruiken en energiezuinige apparatuur aan te schaffen. Elk klein beetje helpt en samen kunnen we een groot verschil maken.', {assignmentId++})");
            
            // Les 3 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Ingevolge artikel 6:162 van het Burgerlijk Wetboek is degene die jegens een ander een onrechtmatige daad pleegt volledig aansprakelijk. Het causale verband tussen de gedraging en de ontstane schade dient adequaat te worden vastgesteld conform de geldende rechtspraak. Voornoemde wettelijke bepalingen zijn van dwingend recht en kunnen niet contractueel worden uitgesloten door partijen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Volgens artikel 6:162 BW is degene die een onrechtmatige daad pleegt jegens een ander, verplicht de schade die hierdoor ontstaat te vergoeden. Dit houdt in dat er een rechtstreekse relatie moet zijn tussen de daad en de schade. Tevens kunnen contractuele afspraken deze wettelijke bepaling niet uitsluiten.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Bij het aangaan van een overeenkomst komt er tussen partijen een vertrouwensrelatie tot stand. Elk der partijen is verplicht zich te gedragen op een wijze welke de ander redelijkerwijs mag verwachten. Bij wanprestatie is de tekortkomende partij aansprakelijk voor de daaruit voortvloeiende schade.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De rechter oordeelt dat de gedaagde schuldig is aan het niet nakomen van de betalingsverplichting. Het is in strijd met de goede trouw om zich op het standpunt te stellen dat de overeenkomst nietig is. De eiser heeft recht op schadevergoeding wegens geleden verlies.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het niet naleven van fiscale verplichtingen kan leiden tot Boetes en rente over het verschuldigde bedrag. In ernstige gevallen kan ook strafvervolging plaatsvinden. Het is van groot belang om tijdig en volledig aangifte te doen van alle inkomsten en bezittingen.', {assignmentId++})");
            
            // Les 4 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De fundamentele vraag naar de essentie van het zijn heeft filosofen door de eeuwen heen intensief beziggehouden met existentiële contemplatie en diepe reflectie. Kant''s transcendentale idealisme postuleert helder dat onze kennis beperkt is tot de wereld zoals die aan ons verschijnt. De dialectische spanning tussen rationalisme en empirisme kenmerkt de epistemologische discours van de moderne filosofie volledig.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Deze klassieker uit de literatuur verkent de thema''s van goed en kwaad door het verhaal van een oprechte held die ten strijde trekt tegen onrecht. Zijn morele en ethische overtuigingen worden op de proef gesteld in een wereld vol corruptie en bedrog. Een diepgaand verhaal dat aanzet tot nadenken over de menselijke natuur.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('In zijn beroemde toespraak legt Martin Luther King uit waarom hij een droom heeft van een betere, rechtvaardigere wereld. Zijn woorden zijn doordrenkt van passie en kunnen zelfs vandaag de dag nog harten en geesten raken. Een must-read voor iedereen die gelooft in gelijkheid en gerechtigheid.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De evolutietheorie van Charles Darwin veranderde voorgoed ons begrip van de menselijke oorsprong en ons behoud. Het boek beschrijft zijn fascinatie met de natuur en de impact van natuurlijke selectie. Het lezen ervan biedt inzicht in de redenen achter de diversiteit van het leven op aarde.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Nieuws is een verslag van actuele gebeurtenissen dat wordt gepubliceerd in kranten, tijdschriften of online platforms. Het biedt feitelijke informatie over wat er in de wereld gebeurt, inclusief politiek, economie, gezondheid en entertainment. Nieuws kan ook opiniepieces en analyses van gebeurtenissen bevatten.', {assignmentId++})");
            
            // Les 5 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Bij de installatie van het softwarepakket dient u ervoor te zorgen dat alle systeemvereisten nauwkeurig worden voldaan voorafgaand aan de implementatie. De configuratie van de netwerkparameters vereist administratieve privileges en een grondige kennis van TCP/IP-protocollen en netwerkarchitectuur. Raadpleeg de troubleshooting guide indien u foutmeldingen ontvangt tijdens het installatieproces van deze software.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Zorg ervoor dat je computer voldoet aan de minimale systeemvereisten voordat je nieuwe software installeert. Dit voorkomt compatibiliteitsproblemen en zorgt voor een soepele werking van het programma. Het is ook aan te raden om een back-up te maken van je gegevens voor het geval er iets misgaat tijdens de installatie.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Volg de installatie-instructies zorgvuldig op om problemen te voorkomen. Als je niet zeker bent van bepaalde instellingen, zoek dan online naar aanbevelingen of vraag om hulp op forums. Na de installatie, test de software grondig om er zeker van te zijn dat alles naar behoren functioneert.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Update regelmatig je software om te profiteren van de nieuwste functies en beveiligingsverbeteringen. De meeste programma''s bieden een automatische updatefunctie aan. Het negeren van updates kan leiden tot kwetsbaarheden in je systeem.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Neem de tijd om je vertrouwd te maken met de nieuwe interface na een software-update. Soms kunnen locaties van knoppen of menu''s veranderen. Een korte rondleiding door de nieuwe functies kan nuttig zijn. Lees de releasenotes voor belangrijke wijzigingen.', {assignmentId++})");
            
            // Les 6 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('In het heldere maanlicht dansten schaduwen mysterieus, het gefluister van de stille nacht, de stilte spreekt welsprekend tot de ziel. Bloemblaadjes dwarrelen langzaam neer als confetti van voorbije dromen en vergeten beloften uit het verleden. De rivier van tijd stroomt onverbiddelijk voort, meeslepend alles wat ooit betekenis had in ons bestaan door de geschiedenis heen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Sterren twinkelen aan de hemel als diamanten op een flonkerende sjerp van samenzijn. Elke gloed vertelt een verhaal van hoop, liefde en onvertelde geheimen. Laten we samen onder deze sterrenhemel dromen van een betere wereld, hand in hand, hart voor hart.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Ogen zijn de vensters van de ziel, zo zeggen ze. Ze kunnen meer zeggen dan duizend woorden, soms zelfs zonder een geluid te maken. Door gewoon in elkaars ogen te kijken, kunnen we een diepere verbinding voelen, een begrip dat verder gaat dan taal.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Licht als een veertje, sterk als een rots. Zo voelen sommige mensen als je ze voor het eerst ontmoet. Onder een schijnbaar kwetsbare buitenkant kan een onwankelbaar doorzettingsvermogen schuilgaan. Het is een eer om zulke vechters te leren kennen op onze levensreis.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Melancholie kan soms als een zware deken op ons drukken, onze stappen vertragen en onze glimlach doen vervagen. Maar zelfs in de somberste tijden, herinneren kleine dingen ons aan de schoonheid van het leven. Een vriendelijk gebaar, een mooi landschap, een dierbare herinnering.', {assignmentId++})");
            
            // Les 7 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Tijdens de bloeiperiode van de Renaissance onderging Europa een ongekende culturele en intellectuele transformatie die de fundamenten legde voor de moderne beschaving. De herontdekking van klassieke teksten stimuleerde een hernieuwde belangstelling voor humanistische waarden en wetenschappelijke inquiry bij geleerden. Prominente figuren zoals Leonardo da Vinci en Michelangelo belichaamden perfect het ideaal van de Renaissance-mens.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De Kabuki-theatertraditie in Japan, met zijn unieke en dramatische vormgeving, biedt een fascinerende kijk op de Japanse cultuur en geschiedenis. Het gebruik van oud verhaalvertel technieken gecombineerd met muziek en dans, maakt het een onvergetelijke ervaring. Zelfs vandaag de dag blijft Kabuki een belangrijk cultureel symbool in Japan.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De ontdekking van penicilline door Alexander Fleming markeerde het begin van de moderne antibioticumtherapie. Dit leidde tot een revolutie in de geneeskunde en heeft miljoenen levens gered sindsdien. Het benadrukt ook het belang van ons ecosysteem, aangezien velen van onze medicijnen oorspronkelijk uit natuurlijke bronnen zijn gehaald.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De evolutie van de technologie in de afgelopen eeuw is niets minder dan verbazingwekkend. Van de uitvinding van de telefoon tot de opkomst van het internet, deze innovaties hebben onze manier van leven en communiceren ingrijpend veranderd. Het is intrigerend om na te denken over wat de toekomst nog meer voor ons in petto heeft.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Cultuur is een dynamisch en continu proces van creatie, bewaard blijven en verandering. Het is de som van de gewoonten, tradities, kunstvormen en waarden die een groep mensen deelt. Cultuur verbindt mensen en biedt een gevoel van identiteit en continuïteit.', {assignmentId++})");
            
            // Les 8 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De premisse dat technologische vooruitgang onvoorwaardelijk leidt tot maatschappelijke verbetering is een discutabele aanname die kritisch onderzocht dient te worden. Enerzijds faciliteren innovaties ongekende mogelijkheden voor communicatie en efficiëntie, anderzijds genereren zij nieuwe vormen van sociale fragmentatie. Een genuanceerde analyse vereist het zorgvuldig afwegen van zowel de potentiële voordelen als de inherente risico''s.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Een kritische benadering van technologie benadrukt de noodzaak voor ethische overwegingen en verantwoord gebruik van technologische middelen. Dit omvat het waarborgen van privacy, het vermijden van afhankelijkheid en het handhaven van menselijke controle over belangrijke beslissingen. Educatie en bewustwording zijn hierin cruciaal.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Wetgeving en beleid moeten gelijke tred houden met de snelle technologische ontwikkelingen om burgers te beschermen tegen mogelijke misbruik. Dit vereist een proactieve benadering van overheden en organisaties wereldwijd. Samenwerking en dialoog zijn essentieel om tot effectieve oplossingen te komen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Als consumenten hebben we de kracht om bedrijven aan te moedigen ethischer en duurzamer te opereren door bewuste keuzes te maken en ons uit te spreken tegen schadelijke praktijken. Onze stem en ons koopgedrag kunnen aanzienlijke veranderingen teweegbrengen in hoe producten worden gemaakt en gedistribueerd.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Bij het bespreken van sociaal-maatschappelijke thema''s is het belangrijk om naar verschillende perspectieven te kijken en een open dialoog te voeren. Dit bevordert begrip en kan leiden tot meer doordachte en inclusieve besluitvorming. Iedereen heeft het recht om gehoord te worden en bij te dragen aan de samenleving.', {assignmentId++})");
            
            // Les 9 - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('De protagonist bevond zich diep in een existentiële crisis, verscheurd tussen de verlangens van het hart en de strenge dictaten van de rede. In de surrealistische setting van het verhaal verweven zich dromen en werkelijkheid tot een caleidoscopisch geheel van betekenislagen. De auteur hanteert vakkundig een stream-of-consciousness techniek om de innerlijke turbulentie van het personage te verbeelden.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Verlies van een dierbare kan een diepgaande impact hebben op een persoon''s psyche en emotionele welzijn. Rouw is een complex proces dat voor iedereen anders is. Het kan tijd kosten om te helen en verder te gaan met het leven, en dat is volkomen normaal.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Herinneringen zijn vaak de enige schatten die ons resteren na het verlies van een dierbare. Het is belangrijk om deze herinneringen te koesteren en te vieren het leven dat geleefd werd. In het bijhouden van een dagboek of een fotoboek kan een troostrijke bezigheid zijn.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Tijdens zware tijden kan het zoeken naar betekenis en doel in het leven ons helpen om door te zetten. Dit kan worden bereikt door meditatie, zelfreflectie of gesprekken met vertrouwde vrienden of een professional. Het is een teken van kracht om hulp te zoeken en te accepteren.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Kunst en literatuur bieden vaak een uitlaatklep voor het verwerken van complexe emoties en ervaringen. Het creëren of gewoon genieten van een kunstwerk kan therapeutisch zijn en nieuwe manieren van begrijpen en genezen ontsluiten.', {assignmentId++})");
            
            // Les 10 (Toets) - 5 variaties
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Deze afsluitende toets evalueert grondig uw volledige beheersing van complexe tekststructuren en uw vermogen tot nauwkeurige en efficiënte tekstverwerking onder tijdsdruk. Demonstreer uw expertise door foutloos te typen met behoud van een hoge snelheid gedurende de gehele opdracht zonder fouten. Het succesvolle afronden van dit niveau getuigt van uitzonderlijke typvaardigheid enkunde op het hoogste niveau.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Bereid je goed voor op deze laatste toets door alle vorige lessen en oefeningen te herzien. Zorg ervoor dat je vertrouwd bent met alle soorten teksten en opdrachten die we hebben behandeld. Dit is je kans om te laten zien hoeveel je hebt geleerd.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Het is essentieel om tijdens deze toets geconcentreerd en gefocust te blijven. Beheer je tijd effectief om alle vragen binnen de gegeven tijd te kunnen beantwoorden. Vergeet niet om je antwoorden altijd nog een keer te controleren op eventuele typfouten of fouten.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Deze toets is ontworpen om niet alleen je typsnelheid, maar ook je leesvaardigheid, concentratie en probleemoplossend vermogen te testen. Neem de tijd om elke opdracht zorgvuldig te lezen en zorg ervoor dat je begrijpt wat er gevraagd wordt voordat je begint met typen.', {assignmentId++})");
            insertQueries.Add($@"INSERT OR IGNORE INTO PracticeMaterials(Sentence, AssignmentId) VALUES('Na het indienen van je toets, ontvang je een gedetailleerd overzicht van je prestaties, inclusief sterke punten en gebieden die verbetering behoeven. Gebruik deze feedback als een waardevolle gids voor je verdere ontwikkeling en groei.', {assignmentId++})");

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
