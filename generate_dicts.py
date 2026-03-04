#!/usr/bin/env python3
"""
Genera dizionari italian.json e english.json con ~8000 parole ciascuno.
Aggiunge varianti morfologiche (plurali, coniugazioni, forme aggettivali).
"""
import json, re, os

# ---------------------------------------------------------------------------
# DIZIONARIO ITALIANO - parole base
# ---------------------------------------------------------------------------
ITALIAN_BASE = """
acqua acre acido acne acuto adatto adottare adulto affetto affitto afflusso
agente agosto aglio agnelio ago agone aglio agopuntura agricoltura alba album
alce alito allora alluce allume aloe alpe alta alto altra altro ambito amico
amore ampio amuleto analisi anello angelo angolo anime anno ansia antenna
anticipo apertura aquila arabo arco area arena argento aria arma armonia
arte artista asino astro astuto aula auro azione azzardo azzurro
bacio ballo bambino banana banco barca barile barone barriera bastone battaglia
batteria battito bellezza bene birra bisogno bocca boccone bordo bosco bottone
braccio bracciale brivido bronzo burro buio
caccia caduta calore camera campagna campo cancello canto capitolo capo
cappello carota carta cartella caso castello catena caverna cena
cerchio cervello ciliegia cipolla clima codice colle colore commento
componente condizione confine conquista contenuto controllo coppia corona
corridoio corsa cortile costo credito cristallo croce cucchiaio cucina
cuore custode danno davanzale debito decisione decorazione denaro deposito
deriva destino dettaglio divano dubbio durata edificio effetto energia enigma
erba esame esercizio esempio esperienza esplosione estate ettaro
fabbrica fama fase favola febbraio fede fedele ferrovia festival fiamma
fibra figura firma fiore fiume flusso fondamento fonte foresta forma
formula forno fossa frana frontiera fuga funzione futuro
galleria gara generazione gestione ghiaccio giardino giocatore gloria governo
grazia grido gruppo guardia gusto
indagine indice industria inferno insieme inizio intervista invito isolamento
istinto istruzione lago lampo lettera libro limite linea livello locale
logica lotta luna luogo
macchina magia manuale marca margine maschera massima materia mattina
memoria mente messaggio metodo mezzo misura modello modifica momento
moneta montagna mostra motivo motore movimento municipio
nazione nebbia nemico notizia nucleo numero obiettivo onore opera ordine
organo origine oscuro
palco pantera patrimonio pensiero percorso pericolo periodo piano pianta
pietra piede pista pizza portico posizione potenza pratica pregio premio
presenza pressione problema processo prodotto profilo progetto
qualità quota realtà regione relazione ricerca richiesta risorsa ritmo
rivalità ruolo rumore salario saldo scala scenario schema scopo scena
senso servizio sessione sfida simbolo sistema situazione sole sorella
spazio spettacolo spirito struttura studio stile strategia suono superficie
talento tassa tattica tema territorio testo tipo titolo traccia
traduzione trasporto tratto unione unità universo valore vantaggio
variante verita versione villaggio visione voce volume zona
abito accento accesso artigiano ascolto aspetto assenza attacco attenzione
attività attore attrezzatura avviso azzardo
bocchino bracciale bugiardo
calcio canzone cavallo cioccolato colore coperta
dente deserto disegno dolce dolore
elefante emozione entrata equilibrio errore
faccia falco famiglia farfalla fatica felicità figura figlio finale
fondazione forte fortuna freccia fumo
gallina galoppo gemello ghepardo gioia giornale giulivo
idioma immagine impatto impegno impresa infinito ingresso inno
laboratorio labirinto lacuna lama lancio lavoro legame lettura luce
maestra maggio mandorla mantello martello marchio
nascita navata negozio nervo nido
ombrello onde ospite
palla pallone palude panico pannello pantano paperino paradosso
parola passione patata pavone pelliccia pencil perla pesca
piccione pilota piuma poesia polpo polvere pompa popolare porto
puzzle
racconto radice ragazzo rallentamento ramo rapido rasatura
rivoluzione roccia rosa rosso
sabbia salute salve scimmia segreto semaforo senato sepolcro
seta sfida sigaretta silenzio sintesi sirena soddisfazione sole
spalla spezia spirale splendore stazione stivale storia
talpa tana tardivo tavolo telescopio tempo tenda terra topo torta
tradizione treno tribunale trovatore
uccello uscita
valigia vampiro vapore variazione velocità vernice vetta viaggio
violino virus vista vittoria
zanzara zebra zucchero zucca zuppa
abete abbigliamento abbondanza abbraccio abitudine abilità
accoglienza accordo aceto aglio aglio aggettivo agosto
albergatore allegria allodola alunno amalgama ambizione
ambasciata amicizia ammonire anfibio angoscia annuncio
aperitivo applicazione aprile archivio ardore armatura
armonioso arpa artrite ascensore assicurazione
atletica attitudine aurora autostrada avventura avventuriero
azzurro acciaio acrobata affacciarsi affrontare

bambina barba barbiere barile barca barriera basamento basso
belva bevanda biblioteca bicicletta bilancio binario biondo
bisonte bolla borsa bosco buio bussola
cabina caffè calcolare calendario callo
cammino campanile canale candidato cannella canto capitano
carabiniere carburo carena carica carisma
carta caserma caserma cassetto castello cavallo caverna cavo
celebrazione centinaia cerimonia certezza cervello chiave
chiodo cielo cilindro circolo citazione civile
classe classico cliente clima codice collega colonna combattente
cometa commedia compagno competizione componente condotto
confronto conquista consiglio contenuto contratto coperta
coraggioso cortile cosmo costume credere cristallo croce
cultura cupola curiosità
decennio decisione dedizione delicato deposito deriva
desiderio destinazione diluvio dimora dinosauro direzione
discordia discriminazione diversità dolcezza dominio dono
drago duello duplice

eco economia educazione elettrico eleganza elmo empatia
emozionante erba eroismo eruzione estetica eterno
evolvere evoluzione esplorazione espressione estensione

fantasia fanfarone fantoccio farfalla faraone faro fede
feroce fertile fibra fiducia flama flanella flauto
fonte forgia forgiatore forte fortezza frammento freccia
freschezza frigorifero frontiera fronte fuoco fuso futuro

galassia gallo gambo gara garante gatto geloso genio
gesto ghepardo giocattolo giornata giovanile gloria
grazia grimoire guerriero guadagno guanto

harmonia habitat idea identità ignoto illuminazione
illustrazione immagine immenso impatto incantesimo
indagine inizio innocente insegnante intelligenza
intrepido intuizione inventore isolamento

karma labirinto laguna lama lancio lastra lavagna
leggenda lemma leone leopardo libertà libro
logica longevo lottare luce luminoso lupo

maestra magia maestria mantello mappa maschera
massimo medico meditazione melodia mentore metallo
minaccia miraggio mistico mistero modulo mondo
muscolo muto

narratore navicella nebbia nectar necromante nero
nessuno nobile nome norma

ombra opportunità oscurita ovest

padre pala palazzo parabola paragone patto
pellegrino pianeta pieta pila pilastro piuma
polso porto potere preghiera prima prigione prigioniero
profondo progetto promessa prova

reame resistenza rispetto rito rituale
rocciosa roccia rossore runa

sacerdote sacrificio saggezza sala saluto salvatore
sano santuario sapere sasso sconfitto seguace senso
serenità sfida sfinge signore simbolo sintonia
sogno soldato solitario spiaggia spirito splendore
stelle storia strategia stregone struttura
sudore supremo sventura

talmente tappa torre tramonto trono

ultima ura

vendetta verità vero vetro vigile villan viottolo
viscere vitalità vittima voto

wisteria

zampillo zanna

abile abito abitudine accorgimento accusare adattare
affermare agire aiutare alzare amare apprendere arrivare
aspettare attendere avanzare avere avviare

ballare bere brindare bruciare

cadere camminare cantare capire cercare chiamare
chiudere cominciare costruire credere

dare decidere dire diventare domandare dormire

entrare essere evitare

fare fermare finire fondare

giocare giungere godere guardare

imparare iniziare insegnare

lavorare leggere liberare

mangiare migliorare morire mostrare muovere

nuotare

osservare ottenere

parlare partire perdere portare potere preferire
prendere preparare promettere proteggere

raccogliere raggiungere ricevere rimanere rispondere
rompere

salire saltare sapere scegliere scoprire scrivere
seguire sentire sognare studiare

tornare trovare

uscire usare

valere vedere vendere venire vivere volare

abbassare abbracciare abituare accarezzare accettare
affrontare aggiungere aiutare aprire ascoltare

battere bloccare

capovolgere colpire combinare confrontare controllare
convincere coprire correggere correre

difendere dimenticare distruggere

eliminare emergere entrare esplorare

fallire filtrare fissare

generare gestire guidare

ignorare illustrare immaginare incontrare indicare
ingannare inseguire interpretare

lasciar liberare

meravigliare migliorare misurare

nascondere navigare

offrire opporre organizzare

passare pensare perdere piegare pizzicare prevalere
produrre promuovere provare pubblicare

rallentare regalare risolvere

sfidare sfruttare sistemare sopravvivere spostare spingere

tagliare toccare trasformare

unire utilizzare

vincere

accostarsi accendere accompagnare accorrere accusare
affidare affrancare aggiornare allenare amareggiare
amplificare annegare antagonizzare appoggiare arricchire

benedire bloccare bruciore bugiardo

catturare combattere comunicare condividere costruire

dimenticare dominare

eleggere emanare emergere eseguire esprimere

fabbricare fantasticare fermare fondare forzare

garantire

illuminare indurre inseguire introdurre

liberare lottare

manipolare meritare migliorare moltiplicare motivare

navigare nutrire

osare ostacolare

perseguire pianificare potenziare praticare procedere
proteggere punire

riconoscere ricordare ridere riunire rivelare

sabotare sacrificare salvare sconfiggere separare
sistemare sopportare sospettare svanire

temere tendere trascinare tuffarsi

urtare

valorizzare vincere

abbandonare accellerare accogliere
battere bloccare bruciare
catturare conquistare costruire
difendere distruggere dominare
emergere esplorare
fallire forzare
guardare guidare
immaginare infiammare insegnare
lavorare liberare
minacciare muovere
navigare nutrire
offrire oppure
perdere potere
raggiungere resistere rompere
sfidare sfruttare superare
trovare
unire uscire
valorizzare vivere
"""

# ---------------------------------------------------------------------------
# Varianti morfologiche italiane
# ---------------------------------------------------------------------------

def italian_variants(words):
    variants = set()
    for w in words:
        w = w.strip().lower()
        if len(w) < 3:
            continue
        variants.add(w)
        # Plurali semplici
        if w.endswith('o'):
            variants.add(w[:-1] + 'i')
        elif w.endswith('a'):
            variants.add(w[:-1] + 'e')
        elif w.endswith('e'):
            variants.add(w[:-1] + 'i')
        # Femminile aggettivi
        if w.endswith('o'):
            variants.add(w[:-1] + 'a')
        # Forme diminutive comuni
        if len(w) > 5 and w.endswith('o'):
            variants.add(w[:-1] + 'ino')
            variants.add(w[:-1] + 'etti')
        if len(w) > 5 and w.endswith('a'):
            variants.add(w[:-1] + 'ina')
        # Forme aumentative
        if len(w) > 4 and w.endswith('o'):
            variants.add(w[:-1] + 'one')
        # Verbi -are → coniugazioni
        if w.endswith('are') and len(w) > 5:
            stem = w[:-3]
            for suf in ['o','i','a','iamo','ate','ano','ato','ata','ati','ate']:
                variants.add(stem + suf)
        # Verbi -ere → coniugazioni
        elif w.endswith('ere') and len(w) > 5:
            stem = w[:-3]
            for suf in ['o','i','e','iamo','ete','ono','uto','uta']:
                variants.add(stem + suf)
        # Verbi -ire → coniugazioni
        elif w.endswith('ire') and len(w) > 5:
            stem = w[:-3]
            for suf in ['o','i','e','iamo','ite','ono','ito','ita']:
                variants.add(stem + suf)
    return {w for w in variants if len(w) >= 3 and w.isalpha()}

# ---------------------------------------------------------------------------
# DIZIONARIO INGLESE - parole base
# ---------------------------------------------------------------------------
ENGLISH_BASE = """
able about above action add age air all allow almost along also always
amount animal another answer any area arm around ask away
baby back bad ball band base basic bear beat become begin behind
believe below best between big black blood blue boat body book born both
break bring brother build burn call came care carry catch cause change
child city class clear close cold come common complete control
cool copy could course cover create cross cut
dark data dead deal deep design direct done door down draw dream drive
drop earth easy end enemy enough ever every evil eye
face fact fail fall family far fast feel feet field fight fill find fire
fish flight floor flow fly fold food force form found free from front full
game give glass go good got great green grow guess hand hard have head
hear heart heavy help here high hold home hope horse hot house
idea image important improve include iron
join just keep key kind king known land large late lead learn left level life
light like line list live long look lost love low
made main make many mark matter mean meet mind miss move music must
need never night north now
often open order other outside over
pack page past path pay people pick piece place plan play plus power
present pretty pull push put question quite
race read real red rest right rock room root rule run
safe save see set shape show side sight sign skill sky sleep slow small
so some sort sound south space stand start stay step still stop store
strong study sun sure
table take talk task teach team tell text thank think time today together
tone tool town trade tree try turn type under
use value very view walk want war watch water wave way week well white whole
wide wild will win wind wish with word work world write wrong
year yet young
above action across actually adjust afraid again against age ahead also
among amount ancient anger another answer apart appear apply area
argue around attack attract avoid aware away

balance base battle bear beautiful best between black blade blood blow blue
bold bone born both break bridge bright bring broke broken build built burst

capture carry castle cause chain change chase clear climb close come command
common complete control cool cross current

damage danger dark dead deal decide deep defeat desert destroy detail
difference different direct distance divine doom dragon draw dream drop dust

early earth easy edge emerge endless enemy enough entire escape even ever
evil evolve exact explore extreme eye

faith fall famous fear find fire flame flash flee float fly follow force
forest found free friend from front full future

game giant give glory gold good grace great green group grow guard guide

hard heal heart help hero high hold hope horn horse hunt

idea impact important include increase inner iron

just keep key kill king know

large last law lead learn left level light lightning lion live long look lose
love low loyal

magic main make master matter mean meet mentor might mind mission move
mountain

nature never night noble north

old open order other overcome

pack path peak place plan power present protect push

quest quick

race range reach ready real realm remain rest right rise rival run

safe save scale search seek send shadow show skill sky slow small
solid soul south speak speed spirit split start stay stone stop storm
story strong survive

tale task team think thunder time together tone tough tower trade train

under unique

valor victory vision voice

walk wall want war watch water wave web wild win wish word world

ability achieve action adapt advance ally ancient armor arrow art

balance band barrier base battle blade block bond brave

capture castle catch chain champion charge clash clear climb conquer control

defend defeat design destiny divine door dream

earthen edge elite endure energy evolve explore extreme

faith fame flame force forest found free forge friend

gather gem giant gift glory goal grace great grid grow guide

harm heal heavy honor hope host hunt

ignite impact improve inspire iron

join journey just

keen keen kind kingdom know

land launch lead legend level light lion lone loyal

magic maker mark master might mount myth

nature never noble north

occupy one open oppose order overcome power prey

quest quick quest quickly

range reach realm reflect resist resolve rise rock rule

safe sage scale seek shadow skill slow smash soul speed
spirit stone storm strength strong

talent tame teach throw thunder time trade trail

unique unite valor valor vision

wall warrior watch wave wild win wise wolf world

ache aide ally arch area aura

bane base bath bone boot bow

cage claw core cult

dare dawn daze deep dive dome dusk dust

ease edge fate fear feel fend foe foul fuse

gear glow grip grow gust

haze heal hero hide hone hook howl hunt

idle iris isle

keen lair land lash lash lore lure

mane mark mend mesh mind mine mist moan moan monk moon

nape nerve nest norm

oath orb

pace pack peak pyre path peak

rage rail rant rave reef reek rein rend rift riot roan roam

sage salt sap scar scry seal sera slay snap soar soul spar spin spire
spit spur stir stun swam

tame taunt thorn tide tone tore trek

urge

vain veil vex vow void

wade wail ward warp welt will wit wolf wrath wring

yell

zone

able agent aisle alert alive allow aloft alone among ample

blast blend bliss bloom blot blur

calm clear clench close coil cold cool core

dart dense dire dire doom drag drift dusk dust

eerie elder ember empty

faint feral feral fierce firm flame flash fleet fleet foe

gain gale gaze given gleam gloomy glow gnarled grit

harsh haste haste heavy hollow howl

idle inner

jagged keen kindle

lack lance large lash lean lean leap lofty lone lost loud

marsh might mist mourn murky

pale peer plunge prow

quell quench quest

raid rank rare raw raze rend rife rigid rise roar rough

scar scout sense sharp sheer slash sleek slim slow smite spell steady

taunt tense thin thorn

unleash utter

veil vigor vital

wary wield wild wilt wrath
"""

def english_variants(words):
    variants = set()
    irregular_plurals = {
        'man':'men','woman':'women','child':'children','person':'people',
        'tooth':'teeth','foot':'feet','mouse':'mice','goose':'geese',
        'leaf':'leaves','wolf':'wolves','half':'halves','life':'lives',
        'knife':'knives','wife':'wives','self':'selves','elf':'elves',
        'shelf':'shelves','loaf':'loaves','thief':'thieves',
    }
    irregular_past = {
        'run':'ran','come':'came','see':'saw','go':'went','take':'took',
        'make':'made','know':'knew','think':'thought','bring':'brought',
        'find':'found','give':'gave','hold':'held','keep':'kept',
        'leave':'left','lose':'lost','meet':'met','say':'said',
        'send':'sent','stand':'stood','tell':'told','win':'won',
        'write':'wrote','break':'broke','build':'built','buy':'bought',
        'catch':'caught','choose':'chose','draw':'drew','drink':'drank',
        'drive':'drove','eat':'ate','fall':'fell','feel':'felt',
        'fight':'fought','fly':'flew','forget':'forgot','get':'got',
        'grow':'grew','hang':'hung','hear':'heard','hide':'hid',
        'hit':'hit','hurt':'hurt','lay':'laid','lead':'led','let':'let',
        'lie':'lay','light':'lit','mean':'meant','pay':'paid',
        'put':'put','read':'read','ride':'rode','ring':'rang',
        'rise':'rose','sell':'sold','set':'set','shoot':'shot',
        'sing':'sang','sink':'sank','sit':'sat','sleep':'slept',
        'speak':'spoke','spend':'spent','spread':'spread','steal':'stole',
        'strike':'struck','swim':'swam','swing':'swung','teach':'taught',
        'tear':'tore','throw':'threw','understand':'understood',
        'wake':'woke','wear':'wore','weep':'wept',
    }
    for w in words:
        w = w.strip().lower()
        if len(w) < 3 or not w.isalpha():
            continue
        variants.add(w)
        # Irregular plural/past
        if w in irregular_plurals:
            variants.add(irregular_plurals[w])
        if w in irregular_past:
            variants.add(irregular_past[w])
        # Plurals
        if not w.endswith('s'):
            if w.endswith(('s','x','z','ch','sh')):
                variants.add(w + 'es')
            elif w.endswith('y') and len(w) > 3 and w[-2] not in 'aeiou':
                variants.add(w[:-1] + 'ies')
            elif w.endswith('f'):
                variants.add(w[:-1] + 'ves')
                variants.add(w + 's')
            elif w.endswith('fe'):
                variants.add(w[:-2] + 'ves')
            else:
                variants.add(w + 's')
        # Verb forms (3rd person, past, gerund)
        if w.endswith('e') and len(w) > 3:
            variants.add(w + 'd')         # loved
            variants.add(w[:-1] + 'ing')  # loving
            variants.add(w + 's')         # loves
        elif w.endswith(('b','d','g','m','n','p','r','t')) and len(w) > 3 and w[-2] in 'aeiou' and w[-3] not in 'aeiou':
            variants.add(w + w[-1] + 'ed')   # stopped
            variants.add(w + w[-1] + 'ing')  # stopping
            variants.add(w + 's')
        elif not w.endswith(('s','x','z')):
            variants.add(w + 'ed')
            variants.add(w + 'ing')
            variants.add(w + 's')
        # Comparative / superlative (short words)
        if len(w) <= 6 and not w.endswith(('er','est','ing','ed','ly')):
            if w.endswith('e'):
                variants.add(w + 'r')
                variants.add(w + 'st')
            else:
                variants.add(w + 'er')
                variants.add(w + 'est')
        # -ly adverbs (from adjectives)
        if w.endswith(('ful','less','ive','ous','al')) and len(w) > 5:
            variants.add(w + 'ly')
    return {w for w in variants if len(w) >= 3 and w.isalpha()}

# ---------------------------------------------------------------------------
# Extra words for Italian to reach 8000
# ---------------------------------------------------------------------------
ITALIAN_EXTRA = """
abbraccio abbondanza abitudine abilità abbigliamento
accoglienza accordo aceto agopuntura agosto albergatore
allegria allodola alunno amalgama ambizione ambasciata
amicizia ammonire anfibio angoscia annuncio aperitivo
applicazione aprile archivio ardore armatura armonioso
arpa artrite ascensore assicurazione atletica attitudine
aurora autostrada avventura avventuriero azzurro acciaio

bambina barba barbiere barile barca barriera basamento
belva bevanda biblioteca bicicletta bilancio binario biondo
bisonte bolla borsa buio bussola

caffè calcolare calendario campanile canale candidato
cannella canto capitano carabiniere carburo carena carica
carisma carta caserma cassetto castello cavallo caverna
cavo celebrazione centinaia cerimonia certezza cervello
chiave chiodo cilindro circolo citazione civile classico
cliente collega colonna combattente cometa commedia
compagno competizione condotto confronto consiglio
contratto coperta coraggioso cortile cosmo costume
cristallo croce cultura cupola curiosità

decennio decisione dedizione delicato deposito deriva
desiderio destinazione diluvio dimora dinosauro direzione
discordia diversità dolcezza dominare dono drago duello

eco economia educazione elettrico eleganza elmo empatia
eroismo eruzione estetica eterno evoluzione esplorazione

fantasia fanfarone fantoccio faraone faro fede feroce
fertile fibra fiducia fiamma flanella flauto fonte forgia
forgiatore fortezza frammento freccia freschezza

galassia gallo gambo garante geloso genio gesto ghepardo
giocattolo giornata giovanile grimoire guerriero guadagno guanto

habitat idea identità ignoto illuminazione illustrazione
immenso incantesimo innocente intelligenza intrepido
intuizione inventore

karma labirinto laguna lama lancio lastra lavagna leggenda
leopardo libertà luminoso

maestria mantello mappa medico meditazione melodia
mentore metallo minaccia miraggio mistico modulo muscolo

narratore navicella nectar necromante nobile nessuno norma

ombra opportunità oscurita ovest

padre palazzo parabola paragrafo pellegrino pianeta pieta
pilastro polso porto preghiera prigione profezia

reame resistenza rispetto rito rituale runa

sacerdote sacrificio saggezza saluto salvatore santuario
sapere sconfitto seguace serenità spiaggia stelle strategia
stregone sudore supremo sventura

tappa torre tramonto trono

vendetta vetro vigilante villan viottolo viscere vitalità

wisteria

zampillo zanna

acidulo adolescente adornare adrenaline affettuoso agganciare
agguato agile agilità agnello airone alabastro alchimia
alchimista aldilà alieno amalgamare ambone
amplificare anaconda antagonizzare anziano apertura araldo
arbitrare arcobaleno ardente arditezza
argomento aristocratico arma armamento armonioso arpa artista
assalire assistente assistenza astrale astuto attimo attivo
attore avversario avvistare

bagliore balena ballista bambola bandito bardi baratro
bastione battaglia bavaglio belva benedire biforcazione
bisogno boccaglio bollire borseggio bottino bracconiere
brigante brillare bruciore bruma bufalo

calcagno camaleonte capace cardine categoria catapulta
cavaliere cerimonia cherubino chimera ciurma classifica
codardo comandante competere contendere convalidare

dardo deduzione delusione demonico deserto devastare
discepolo dissolversi distogliere dormiente druidico
druido

eccentrico efferato elegante elfico ellisse emissario
emanazione enimma enzima eroe eseguire esorcismo
espediente essenza eternità evasione evocatore evocazione

fanatic fantasma farla ferita feudale fiduciario
fiero fionda flebile fluido folgorare foriero
formidabile fortissimo foschia frangente furtività

galoppo geniale gigante gladio gladiatore gnomo
gracchio grafico guaritore guida

imboscata immunità impavido implacabile infernale
ingenuità inimicizia innato insidia insidioso
intercessore intrepidezza invocazione ipnotico

laggiù lamento larva lavorazione lealtà leggendario
lesto lincea livore logoro lontra

macabro maledetto maleficio malefico malvagità
mandrake mannaro mareggiata maturità melanconia
mercenario messia metamorfosi milite minaccioso
mistico mitico mobilitare moltiplicare morale

nazione nobile nobiltà nomade nordico

oblio omaggio omicidio onnisciente onnipotente oppressione
orbita ordinamento orrore oscuro ostile

palafreniere paramagico partigiano patriarca
pentacolo percettivo pittoresco placido planare potere
presagio prode profetico prurito pugile

rancore rapimento rappresaglia ravvivare redenzione
reliquare reliqua reso rifugio rimedio rinnegato
rituale rivoltoso riunione ronzio ronzire rottame

sabotaggio saggezza salvo sepolcro servitore sguardo
sigillo sinistra soggiogare sortilegio soverchiare
specter spellata spettro squilla stratagem stratagemma

tatticismo tenace tenebre tirapiedi tradimento trance
travolgere tribuno trincea troubadour turbine

ululare ululo ungulato unione urlare

valente valore valoroso vampiro vortice

zuffa zufolate
"""

# ---------------------------------------------------------------------------
# Extra words for English to reach 8000
# ---------------------------------------------------------------------------
ITALIAN_EXTRA2 = """
abbacinare abbaiare abbassarsi abbattere abbellire abbracciare
abdicare abilmente abisso abnegazione abolire aborrire
abrasione abusare accademia accadere accanimento accanito
accedere accelerare accentuare accettare accidentale acclive
accomodare accoppiamento accorciare accorgimento accusare
acerbo achille acque acredine acrobatico adagiare adagio
adamantino addestramento addio addormentarsi adeguato
admirable adorare adottare adulazione affabile affrontare
agganciare aggressivo agguato agilità agonismo
alabastro alchimia alchimista alieno allagamento allarmante
allestire allineamento allocare allontanarsi allungare
alternare ambone amplificare anaconda antagonizzare
apertura araldo arbitrare ardente arditezza argomento
aristocratico armamento armonioso artista assalire
assistente astrale astuto attimo avversario avvistare

bagliore balena ballista bambola bandito bardi baratro
bastione bavaglio belva benedire biforcazione bisogno
bollire borseggio bottino bracconiere brigante bruciore bruma

calcagno camaleonte capace cardine catapulta cavaliere
cherubino chimera ciurma codardo comandante competere
contendere convalidare

dardo deduzione delusione demonico devastare discepolo
dissolversi distogliere dormiente druidico druido

eccentrico efferato elfico ellisse emissario emanazione
enimma enzima esorcismo espediente essenza evasione
evocatore evocazione

fanatic fantasma ferita feudale fiduciario fiero fionda
flebile fluido folgorare foriero formidabile furtività

galoppo geniale gladio gladiatore gnomo gracchio grafico
guaritore guida imboscata immunità impavido implacabile
infernale ingenuità inimicizia innato insidia insidioso
intercessore intrepidezza invocazione ipnotico

laggiù lamento larva lealtà leggendario lesto lincea livore
logoro lontra macabro maledetto maleficio malefico malvagità
mandrake mannaro mareggiata maturità melanconia mercenario
messia metamorfosi milite minaccioso mistico mitico

nazione nobiltà nomade nordico oblio omaggio omicidio
onnisciente onnipotente oppressione orbita ordinamento orrore

palafreniere paramagico partigiano patriarca pentacolo
percettivo pittoresco placido planare presagio prode
profetico pugile rancore rapimento rappresaglia ravvivare
redenzione reliquare reliqua rifugio rimedio rinnegato
rivoltoso riunione ronzio

sabotaggio saggezza salvo servitore sigillo sinistra
soggiogare sortilegio soverchiare specter spettro
squilla stratagem stratagemma tatticismo tenace tenebre
tirapiedi tradimento trance travolgere tribuno trincea
turbine ululare ululo ungulato urlare

valente valoro valoroso vampiro vortice zuffa
abbandonato abbattuto abbellito abituato accampato accantonato
acceso acclamato accolto addebitato addolorato adorato
affranto agitato aggredito allenato allucinato amareggiato
ambientato ammalato ammesso annichilito annientato annoiato
antropologia apparecchio apprendimento appropriato arricchito
assetato attraversato

barbarico benedetto catturato circondato colpito commosso
comunicato condannato confuso controllato corrotto

dannato decaduto descritto distrutto dominato

educato esaurito espresso

ferito fiducioso flagellato

garantito

illuminato immaginato impedito incantato indebolito infuriato
ingannato inseguito invocato

lamentato lasciato liberato

maledetto minacciato motivato

necessitato

offeso oppresso

perduto plagiato potenziato provato punito

raggiunto rovesciato

sacrificato salvato sconfitto sepolto separato sfidato
simboleggiato sopraffatto sospettato superato

temuto tradito trascinato

vinto

accelerare accarezzare accompagnare accorrere affermare aggiornare
allontanare amareggiare amplificare annegare appoggiare arricchire
benedire boicottare catturare comunicare condividere costruire
eleggere emanare eseguire esprimere fabbricare fantasticare
fermare fondare forzare garantire illuminare indurre introdurre
manipolare meritare moltiplicare motivare nutrire ostacolare
perseguire pianificare potenziare praticare procedere punire
riconoscere ricordare ridere riunire rivelare sabotare salvare
sconfiggere separare sopportare sospettare svanire tendere
tuffarsi urtare valorizzare collaborare compensare completare
compromettere condurre configurare consolidare contribuire
coordinare creare delegare elaborare esaminare facilitare
formalizzare generare implementare integrare investire
mantenere massimizzare minimizzare ottimizzare pianificare
potenziare promuovere raccogliere raggiungere reclutare
reinforzare sviluppare trasformare unificare validare


abitare accettare acclamare accusare addormentare affascinare
allenare alzare amare ammirare annunciare apprendere armare
arrossire assalire avanzare baciare bloccare bruciare cambiare
cantare catturare cercare chiamare colpire combattere conquistare
credere crescere danzare decidere dedicare diventare dominare
eliminare emergere esplorare evolvere fermare festeggiare
formare fuggire gareggiare gridare illudere illuminare
imparare inseguire lanciare lottare meravigliare mostrare
muovere nascere nutrire offrire opporre osservare pagare
pattugliare percorrere piegare potere pregare preparare
proteggere raggiungere resistere rispettare riuscire salire
scagliarsi scegliere scoprire segnare sfuggire sognare
sopravvivere sperare spingere studiare temere trasformare
uccidere urlare valutare valorizzare
abbandonare accorgersi affidare agganciare allearsi ammettere
appoggiare arricchire avvicinarsi ballare bloccarsi bruciare
capovolgere colpire combinare confrontare controllare
convincere coprire correggere correre difendere dimenticare
distruggere emergere fallire filtrare generare gestire guidare
ignorare illustrare immaginare incontrare indicare ingannare
interpretare liberare meravigliare misurare nascondere navigare
perseguire piegare pizzicare prevalere produrre promuovere
rallentare regalare risolvere sfidare sfruttare sistemare
sopravvivere spostare spingere tagliare toccare

abbaiare abbiocco abboccare abbrustolire abdicare abilmente abisso
abnegazione abolire aborrire abrasione abusare accanimento accanito
accedere accelerare accentuare accettare accidentale acclive accomodare
acerbo achille acque acredine acrobatico adagiare adagio adamantino
addestramento affabile aggressivo allagamento allarmante allestire
allineamento allocare allontanarsi allungare ambone amplificare
antagonizzare arbitrare ardente arditezza aristocratico armamento
assalire assistente avvistare bagliore ballista baratro bastione
bavaglio benedire biforcazione bollire borseggio bottino bracconiere
brigante bruma calcagno catapulta cavaliere cherubino chimera ciurma
codardo competere convalidare dardo deduzione delusione demonico
devastare discepolo dissolversi dormiente druidico eccentrico efferato
elfico ellisse emissario emanazione enimma esorcismo espediente
evasione evocazione fanatic feudale fiduciario fionda flebile fluido
folgorare foriero formidabile furtività galoppo geniale gladio
gladiatore gnomo gracchio guaritore imboscata immunità impavido
implacabile infernale ingenuità inimicizia innato insidia insidioso
intercessore intrepidezza invocazione ipnotico laggiù lamento larva
lealtà leggendario lincea livore logoro lontra macabro maledetto
maleficio malefico malvagità mannaro mareggiata melanconia mercenario
metamorfosi milite minaccioso mistico mitico nazione nobiltà nomade
nordico oblio omicidio onnisciente onnipotente oppressione orbita
palafreniere paramagico partigiano patriarca pentacolo percettivo
presagio prode profetico pugile rancore rapimento rappresaglia
ravvivare redenzione reliqua rifugio rimedio rinnegato rivoltoso
ronzio sabotaggio salvo sepolcro servitore sigillo soggiogare
sortilegio soverchiare specter spettro stratagem stratagemma
tatticismo tenace tenebre tirapiedi tradimento trance travolgere
tribuno trincea turbine ululare ululo ungulato valente valoroso
vortice zuffa abbandonato abbattuto abbellito abituato affrontato
agitato aggredito allenato amareggiato antropologia attivo avversario
barbarico catturato circondato colpito commosso condannato confuso
dannato decaduto distrutto dominato educato esaurito ferito fiducioso
garantito illuminato impedito incantato indebolito infuriato ingannato
liberato maledetto motivato offeso oppresso perduto plagiato potenziato
punito raggiunto sacrificato salvato sconfitto sfidato superato
temuto tradito vinto abbondantemente acrobazia acutezza adornamento
affascinante agguato agilità agonismo alchimia alchimista alieno
armonioso armatura ardore assistenza astrale astuto attimo bagliore
belva brigantaggio cavalleresco cherubino chimera ciurmaio codardone
coraggiosamente devastazione discepolo druido eccentricità elficamente
ellisse emanazione evocatore fantasma feudalismo fiduciario
formidabilmente furtivamente galoppo gentildonna gladiatore gnomesco
guaritore leggendario lincea logoro lontra macabro maledizione
maleficio mannaro melanconico mercenariato metamorfosi milite
misticamente mitico nobiltà nomade nordicamente oblioso omnisciente
onnipotenza orbita palafreniere patriarca pentacolo percettività
presagio profetico pugile rancore rapimento rappresaglia ravvivare
redenzione reliquiare rimediare rinnegato rivoltoso ronzare
sabotaggio salvatore sepolcro servitore sigillo soggiogare sortilegio
specter stratagemma tatticismo tenebroso tirare tradimento trance
travolgere tribuno trincea turbinare ululare ungulato valoroso
vortice battaglia cavaliere combattimento drago elfiche fantasia
fortezza guerriero leggenda magia mistico mito regno strega vampiro
"""

ENGLISH_EXTRA = """
abduct abide abrupt absent absorb abuse accept access
acclaim accuse ache achieve acquire adapt adhere adjust
admit adore advance advise affirm afford alarm alert
align allege allow alter amaze anchor anger animate
annex appeal appoint arise armor arouse assail assert
assist assure attack atone attract augment avenge avoid
awaken

babble banter banish barrage beckon befall befriend
behold belittle bereft bestow betray bind bless blunder
boast bolster brace brave brawl breach breach bribe
build burden

capture cascade cavern challenge charm chill circle
clamor clash cleave cling coerce collapse combat command
compel confound confront conjure conquer contain corrupt
counsel counter crumble crush curb curse

damage daze decay declare defeat deflect defy delay
demand deny descend describe destroy deter devastate
devour disarm disclose discover dispel disrupt divine
dodge doom drain dread dwell

embark empower enchant endure enforce engulf enrage enthrall
equip escape evade evolve exact exalt exile expose

falter faze feign fend forsake fortify foster found
fracture free freeze fulfill

gather generate glide grieve grind guard guide

hamper haunt heal hinder hold honor hunt

ignite impact impose invoke isolate

judge

kindle know

lament launch lead liberate lure lurk

manipulate meddle merge mimic mobilize motivate mourn

negate neutralize

obliterate obscure offend oppose oppress overcome

parry persevere pierce plunge preserve prevail pursue

rally reave rebuke reclaim redeem release rend rescue
resist retrieve revive rise roam rouse rule

sacrifice scatter scour seize shatter shift siege
slay smite snare soar spare spawn spell stalk
strive subjugate summon surge survive

tame taunt terrorize topple transform trap trial triumph

unleash unravel uphold

vanish vault vex vanquish venture

wander ward weave wield withstand

yielding

abolish absorb accelerate accomplish accuse acquire
adapt administer advance advise advocate affirm
aggregate alarm alert analyze anchor animate annex
appeal apply appoint approach arouse astonish attain
augment awaken

besiege bestow betray bolster brand break bring
build challenge change charge chase collaborate
command compensate compel compete complete compromise
condition conduct confront conjure conquer control
convert corrupt cover craft create crush

damage decide declare defend defer define delay
deliver deploy describe design detect determine
develop divert divide dominate dread

elaborate empower enable encourage enforce engage
enhance ensure establish evaluate evolve execute
expand exploit explore express extend

facilitate fight focus forge form fortify free

gather generate guide

honor hunt

identify ignore implement improve increase influence
initiate inspire integrate invent invoke isolate

justify

launch lead leverage limit

maintain manage master maximize measure mitigate

navigate notify nurture

observe obtain optimize overcome overpower oversee

persevere plan prepare preserve prevent produce protect
provide pursue

recognize recruit reinforce release represent resist
resolve respond retain reveal rise

sacrifice seize shape solve spark stand strengthen
strike support survive sustain

track train transform trust

unify unleash

validate value

withstand

ability action adapt advance aid align amplify anchor
apply arch arm around ascend ask attract aware

balance bare battle beacon bear beat become bind blaze
blot bond born brave breach bring build

call calm catch chain challenge change charge chase cheer
cling close coil collect command commit control cover
craft create

dare deal decide deflect define deliver destroy drive

earn edge empower endure engage enhance entrust evolve
execute exhaust

face fail faith fall fear feel fight find flee follow
force forge forward free fulfill fury

gain give glory grace grow guide

harm heal help hold honor hope hunt

impact inspire invoke

join journey

keep kindle

launch lead learn let live look lure

make mark meet mend might move

need never

offer open oppose

pursue protect prevail

quest quick

race raid raise rally reach rebuild rend resist return
rise roam run rule rush

safe sage save scale seek set shatter shield skill slay
soar soul span speed spirit spread stand start strive
survive

take talent tame taunt teach tend think thwart tide
track train transform travel triumph

unite unleash use

valor vanquish venture

walk ward watch weave wield win wish witness

yield

abyss afoot akin aloft amok anew apex aria ark awe balk bane bask bawl beam bevy bias bilk blab bode boon boor bout brag bray brim brow buck buff bunk buzz cafe calm cant cape carp cast cave cite claw clog clop clot coax coil colt coma cope coup cozy cram crib crux cuff curb curl dab dale damp dart deft dent dice dike dine dire dirk dole dolt dote dove drab drip drub drum dupe eave etch exam faze fend fern fest fife fizz flab flaw flog font ford fork fray fret fume gale gape gasp gawk geld gibe gild gilt gist glee glib glob glut gnaw goon gory gram grin grit grog grout grub gush gust hack hale hank hark heap heave heft helm hemp hewn hick hike hoax hobo hog hone hood hoot horn husk icon inch iris itch jack jade jape jibe jolt josh jot jowl junk keen kelp kiln kink lace lame lard laud laze lean leech leek lilt limp lisp lobe loft loll loot lout lull lurk lust mace malt mane mare marsh mast maw maze mead meek meld mime mire mock molt mooch mope moth mote muck mulch mull musk narc nerd newt niche nick node nosh notch null numb oath ogre omen orb ore oust oven oxen ozone pact pall pang pawn pear peel perk pest pike pile pith plod plop ploy pluck plug plunk poach pock poll pomp pond pore pout prep prey prig prod prow puff pulp punt pyre quake quash quell quip ramp rang rant reef reek rein rend rent riff rile rime rind rink roan romp rout rove ruff rump runt rust sag sail sake sap sash scab scam scan scat scoff scour seam sect seep seer sere sham shed shin shirk shiv shun siege sift sill silt skew skim skip slab slam slat sled slim slit slob slop slot slur smash smear smelt smirk snag snap snare snarl sneak sneer snit snob snoop soak sock soot span spit spook spot spout spry stab stale stall stave stead stew stiff sting stir stomp stool stout strut stub stun suck sulk sully sump swab swam swath swoop tack tamp tank tarn taut teem terse thud tidy till tinge tint toil toll tong trek trim trip troth trudge trump tuck tweak twin twig tyke umbra vale vamp vary vault veer venom verge vim vise wane warp weld whim whin whip whirl whit wick wile wilt wince wink wisp woe woo worm wring yawn yearn yore yowl zest zing zombie balm bask bold dale dash dawn deft dour dusk earl envy epic even fair fawn faze gale hale haven heap hunt iris jest jest keen lash lean lien limb lore lull lure lurk lyre mace maze meek meld moat molt nook opal orb pact palm perch ploy plum pore prey prim prod prow prune quail quash ream reed rind rove rune sage sear sheen shim shin skim slab slew slim slop snag snub sole span spar spit spore spry spur stem stoat stomp strife stout swear swirl tangle taut teem tend terse tier tithe toil tone troth trove trump tuck vale veil vex wane warp whim wisp woe yore zeal
"""

# ---------------------------------------------------------------------------
# Combina e genera
# ---------------------------------------------------------------------------

def clean_words(text):
    return [w.strip().lower() for w in text.split() if len(w.strip()) >= 3 and w.strip().isalpha()]

def build_dict(base_text, extra_text, variant_fn, extra_text2="", target=8000):
    base = clean_words(base_text + ' ' + extra_text + ' ' + extra_text2)
    all_words = variant_fn(base)
    # Ordina alfabeticamente, rimuovi duplicati
    result = sorted(all_words)
    # Se abbiamo più del target, prendi i più corti (più probabili in griglia 5x5)
    if len(result) > target:
        # Preferisci parole brevi (3-8 lettere)
        short = [w for w in result if 3 <= len(w) <= 8]
        long_  = [w for w in result if len(w) > 8]
        if len(short) >= target:
            result = short[:target]
        else:
            result = short + long_[:target - len(short)]
    return result

print("Generazione dizionario italiano...")
italian_words = build_dict(ITALIAN_BASE, ITALIAN_EXTRA, italian_variants, ITALIAN_EXTRA2, 8000)
print(f"  Parole italiane: {len(italian_words)}")

print("Generazione dizionario inglese...")
english_words = build_dict(ENGLISH_BASE, ENGLISH_EXTRA, english_variants, "", 8000)
print(f"  Parole inglesi: {len(english_words)}")

# Output paths
base_path = os.path.join(os.path.dirname(__file__),
    "AppPuzz Games/Assets/Resources/Dictionaries")
os.makedirs(base_path, exist_ok=True)

with open(os.path.join(base_path, "italian.json"), "w", encoding="utf-8") as f:
    json.dump({"words": italian_words}, f, ensure_ascii=False, indent=None, separators=(',',':'))

with open(os.path.join(base_path, "english.json"), "w", encoding="utf-8") as f:
    json.dump({"words": english_words}, f, ensure_ascii=False, indent=None, separators=(',',':'))

print("Done! File scritti in:", base_path)
print(f"  italian.json  → {len(italian_words)} parole")
print(f"  english.json  → {len(english_words)} parole")
