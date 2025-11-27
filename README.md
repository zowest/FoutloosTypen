# Foutloos-typen
# Projectleden

**Mees van Zwolle** | s1213648

**Matthijs Wietsma** | s1213687

**Danial Billows** | s1170068

**Jackie Xie** | S1202909

**Zoë Westerveld** | s1200037

**Anne Dirk van der Weerd**  | s1216449

# Afspraken
-	We hebben afgesproken dat bij elke Pull request een review nodig is van een teamgenoot, dit valt onder het 4 ogen principe.
-	We houden gitflow aan als branch-structuur.
-	We houden voor al onze diagrammen de UML-richtlijnen aan.

## Werkwijze Proces  

Onze Github-omgeving maakt gebruik van drie hoofdbranches:  

- **Main (Productie)** → Bevat stabiele en geteste code die in gebruik is.  
- **Develop** → Hier worden nieuwe features en bugfixes geïntegreerd en getest voordat ze worden goedgekeurd.  
- **Feature Branches** → Elke nieuwe feature of bugfix krijgt een eigen branch gebaseerd op de Test-Acceptatie branch, 
                         met een duidelijke   naam (bijv. `feat/login`, `bugfix/header-layout`).  

### **Workflow en Change Management**  
1. Directe commits naar `Develop` of `Main` zijn niet toegestaan.  
2. Wijzigingen gaan via **Merge Requests (MR's)** en moeten vooraf getest zijn.  
3. Minimaal één ander teamlid moet de MR beoordelen en goedkeuren.  
4. Na goedkeuring wordt de code naar **Develop** gemerged en daar verder getest.  
5. Na klantacceptatie wordt de code klaargemaakt voor **Main/Productie**.  

### **Productiefase**  
- Code die naar productie gaat, moet **stabiel en volledig getest** zijn.  

