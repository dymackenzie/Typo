using Godot;
using System;
using System.Linq;

public partial class Words : Node
{

	private static readonly string[] FOUR_LETTERS = new string[] {"aged", "arab", "atom", "bold", "bush", "buzz", "cafe", "call", "cart", "city", "clay", "club", 
	"coat", "cuba", "cuts", "dame", "deal", "deck", "dell", "deny", "drug", "east", "exec", "expo", "face", "feed", "feet",
	"film", "fish", "flag", "foam", "fork", "gain", "gave", "gene", "grad", "hard", "heat", "hits", "hold", "home", "hull", 
	"jean", "judy", "kept", "king", "kits", "knit", "lamp", "leon", "lips", "lord", "love", "luck", "mail", "make", "male", "mine", "misc", "navy", "odds", 
	"oven", "pads", "park", "pens", "pink", "plan", "polo", "poor", "pork", "push", "read", "rear", "reed", "rico", "rise", "rome", 
	"sake", "sans", "seal", "seen", "semi", "shaw", "song", "sort", "soup", "stem", "sync", "tear", "that", "took", "trio", "twin", "type", 
	"wage", "want", "warm", "ways", "wide", "wire", "wrap", "xbox", "zone"};
	
	private static readonly string[] FIVE_LETTERS = new string[] {"acute", "agent", "aimed", "alien", "alloy", "anime", "asked", "banks", "berry", "billy", "black", "blank", "blood", 
	"booth", "boxed", "cabin", "calls", "cards", "cents", "chain", "charm", "chips", "chose", "cisco", "cohen", "colon", "coral", "costs", "court", "crest", "crown", 
	"dance", "dates", "death", "dodge", "doors", "drive", "ellen", "euros", "evans", "exams", "fails", "fancy", "fatal", "feels", "filed", "first", "flesh", "flood", 
	"flour", "frank", "fresh", "front", "gauge", "genre", "given", "glenn", "grant", "guard", "guide", "honey", "icons", "indie", "items", "jerry", "karen", "knows", 
	"lance", "latin", "level", "limit", "lodge", "loops", "lucky", "lycos", "major", "march", "mason", "merit", "model", "modes", "motel", "mouth", "nikon", "ocean", 
	"opens", "parks", "pasta", "paths", "peter", "picks", "pitch", "ports", "promo", "proof", "pumps", "qatar", "randy", "rates", "ready", "rebel", "rhode", "right", 
	"roger", "royal", "salem", "scoop", "scout", "shape", "shift", "shown", "sixth", "slave", "solar", "start", "stats", "steps", "style", "susan", "swing", "tamil", 
	"texas", "third", "those", "tiles", "tough", "tower", "tribe", "trunk", "tulsa", "uncle", "venue", "video", "volvo", "wages", "wayne", "wheel", "witch", "worse", 
	"yahoo", "yield"};

	private static readonly string[] SIX_LETTERS = new string[] {"active", "actual", "advice", "africa", "albert", "amanda", "annual", "appeal", "arrest", "assume", "asthma", "awards", 
	"backed", "baking", "became", "behind", "bikini", "boards", "boston", "branch", "brunei", "candle", "caring", "carpet", "cement", "checks", "cialis", "coffee", 
	"cooler", "corner", "cowboy", "credit", "crisis", "custom", "danish", "decade", "degree", "denial", "detail", "diesel", "disney", "divine", "donald", "driven", 
	"eleven", "ending", "entity", "erotic", "expert", "factor", "fallen", "faster", "finals", "fitted", "forums", "freeze", "friday", "funded", "garage", "geneva", 
	"gerald", "giving", "global", "guilty", "guyana", "hacker", "happen", "health", "hebrew", "hereby", "hilton", "hiring", "ignore", "inform", "itunes", "jackie", 
	"jeremy", "killer", "kuwait", "labels", "legend", "lesson", "levels", "limits", "locked", "lolita", "looked", "lowest", "mainly", "merger", "mining", "models", 
	"monaco", "morgan", "motion", "murder", "naples", "nathan", "nearby", "neural", "nicole", "norton", "notify", "panels", "pastor", "phrase", "picnic", "pledge", 
	"policy", "posted", "prague", "prison", "prizes", "proved", "public", "racial", "radius", "raised", "rarely", "reggae", "relief", "result", "retain", "ribbon", 
	"rights", "rolled", "routes", "ruling", "russia", "saturn", "saving", "scheme", "scores", "season", "seeing", "sender", "sensor", "serial", "serves", "sewing", 
	"shared", "shower", "silver", "singer", "slides", "slovak", "smooth", "socket", "sphere", "stages", "string", "struck", "sunset", "surely", "suzuki", "tables", 
	"temple", "tennis", "thomas", "though", "timely", "topics", "trains", "treaty", "valves", "vendor", "verbal", "viewer", "warren", "watson", "willow", "wishes", 
	"womens", "writes", "yellow", "zambia"};

	private static readonly string[] SEVEN_LETTERS = new string[] {"achieve", "acrobat", "adapter", "affects", "alleged", "allowed", "ambient", "animals", "antique", "arising", "artwork", 
	"aspects", "attempt", "authors", "bangkok", "baptist", "beaches", "believe", "belongs", "bernard", "binding", "breasts", "britney", "brother", "buffalo", "capture", 
	"casting", "centres", "charges", "cheaper", "chicago", "claimed", "classes", "closest", "college", "columns", "company", "confirm", "console", "consult", "contact", 
	"correct", "counter", "coupled", "creates", "cruises", "cumshot", "deleted", "depends", "diploma", "doctors", "drawing", "earning", "effects", "enables", "engines", 
	"enjoyed", "failure", "ferrari", "florida", "folders", "foreign", "framing", "gratuit", "grenada", "handles", "harmful", "harvard", "hazards", "headers", "holiday", 
	"hormone", "however", "hygiene", "imaging", "impacts", "implies", "insects", "interim", "islamic", "israeli", "italian", "jamaica", "journal", "justice", "karaoke", 
	"laptops", "lasting", "latinas", "lawsuit", "leading", "lighter", "lottery", "managed", "marking", "martial", "members", "mexican", "million", "moldova", "monthly", 
	"nervous", "norfolk", "numeric", "observe", "offered", "opening", "passing", "patient", "payroll", "pendant", "percent", "podcast", "portion", "poverty", "primary", 
	"product", "protein", "ranking", "records", "removed", "rewards", "royalty", "samples", "science", "scripts", "sectors", "shakira", "shortly", "sisters", "smaller", 
	"solomon", "staying", "stevens", "stopped", "sucking", "suicide", "terrace", "thereof", "tickets", "tobacco", "tracked", "tribute", "trouble", "tsunami", "tuition", 
	"uniform", "unusual", "upgrade", "upskirt", "uruguay", "version", "veteran", "village", "violent", "visitor", "wearing", "weights", "worship"};

	private static readonly string[] EIGHT_LETTERS = new string[] {"actively", "aluminum", "analysis", "answered", "antibody", "assigned", "athletes", "attorney", "auckland", "bathroom", 
	"becoming", "behavior", "bookings", "brunette", "bulgaria", "cartoons", "catalogs", "chambers", "chapters", "classics", "cleaning", "clicking", "collapse", 
	"columbus", "combines", "concepts", "consists", "contrary", "creation", "cultures", "darkness", "defining", "delivers", "detector", "develops", "directly", 
	"dispatch", "dramatic", "earliest", "electric", "elements", "entering", "everyday", "expenses", "explicit", "focusing", "football", "function", "gangbang", 
	"grateful", "happened", "highways", "homeland", "illinois", "investor", "involved", "japanese", "judgment", "logitech", "maldives", "manitoba", "marshall", 
	"mercedes", "midlands", "minister", "montreal", "observer", "officers", "overcome", "partners", "pavilion", "plumbing", "pointing", "positive", "postcard", 
	"previews", "probably", "proceeds", "producer", "provider", "reaching", "repeated", "required", "reseller", "resulted", "richmond", "robinson", "roommate", 
	"sandwich", "segments", "shopping", "solution", "staffing", "subjects", "supplier", "supposed", "surround", "symantec", "syndrome", "taxation", "tomorrow", 
	"trainers", "treasury", "uploaded", "vacation", "warnings", "wireless"};

	private static readonly string[] NINE_LETTERS = new string[] {"academics", "addressed", "admission", "adventure", "afternoon", "alexander", "announced", "apartment", "appointed", 
	"arguments", "assembled", "attempted", "bluetooth", "boulevard", "breakdown", "brochures", "communist", "competent", "complaint", "composite", "conflicts", 
	"continues", "convicted", "correctly", "criticism", "democracy", "discovery", "ecommerce", "employees", "essential", "exclusion", "expansion", "expertise", 
	"financing", "following", "forecasts", "freelance", "galleries", "genealogy", "graduated", "hardcover", "hepatitis", "hopefully", "induction", "initially", 
	"installed", "intensity", "intervals", "introduce", "lafayette", "languages", "lithuania", "livestock", "machinery", "namespace", "nashville", "navigator", 
	"nominated", "nutrition", "occasions", "omissions", "operators", "ordinance", "ownership", "paintball", "palestine", "passenger", "peninsula", "personals", 
	"polyester", "precision", "prisoners", "programme", "projected", "protocols", "questions", "receiving", "relevance", "retention", "returning", "revisions", 
	"sacrifice", "sensitive", "separated", "sheffield", "singapore", "sociology", "southeast", "specifics", "spotlight", "standards", "strategic", "traveling", 
	"undertake", "universal"};

	private static readonly string[] TEN_LETTERS = new string[] {"adjustable", "affiliated", "agreements", "ambassador", "appliances", "associates", "automobile", 
	"background", "beneficial", "britannica", "citysearch", "collection", "commercial", "committees", "compliance", "conclusion", "constitute", 
	"contacting", "controlled", "counseling", "currencies", "department", "dictionary", "disclaimer", "editorials", "encryption", "equivalent", 
	"forwarding", "fragrances", "generators", "geological", "impression", "infections", "interested", "irrigation", "meaningful", "mozambique", 
	"phenomenon", "physicians", "publishers", "qualifying", "recognised", "recreation", "referenced", "regression", "relocation", "respondent", 
	"signatures", "statements", "structural", "sufficient", "terrorists", "throughout", "traditions", "transition", "ultimately"};

	private static readonly string[] FIFTEEN_LETTERS = new string[] {"characteristics", "confidentiality", "congratulations", "instrumentation", "internationally", 
	"pharmaceuticals", "recommendations", "representations", "representatives", "troubleshooting"};

	public string[][] WORD_BANK = new string[][] {FOUR_LETTERS, FIVE_LETTERS, SIX_LETTERS, SEVEN_LETTERS, EIGHT_LETTERS, NINE_LETTERS, TEN_LETTERS, FIFTEEN_LETTERS};

	public string GetRandomPrompt(int index) {
		string[] words = WORD_BANK[index];
		// randomize
		RandomNumberGenerator random = new();
		random.Randomize();
		return words[random.Randi() % words.Length].ToUpper();
	}
	
}
