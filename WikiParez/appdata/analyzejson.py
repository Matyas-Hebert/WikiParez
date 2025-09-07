import json
import re

json_data = open('pagesData.json', 'r', encoding='utf-8').read()
data = json.loads(json_data)

bram = [
    "Room Klub is not in the bordering rooms of Byt chudáka, kterej nemá rád kluby",
    "Room Mostiště is not in the bordering rooms of Vídeň",
    "Room Skokanské můstky is not in the bordering rooms of Lázně Panoraama",
    "Room Hvězda is not in the bordering rooms of Kazeta",
    "Room Hvězda is not in the bordering rooms of Dálniční Pilíř",
    "Room Hvězda is not in the bordering rooms of Hnízdo",
    "Room Dálniční Pilíř is not in the bordering rooms of Balónky",
    "Room Středové náměstí is not in the bordering rooms of Kazeta",
    "Room Středové náměstí is not in the bordering rooms of Dálniční Pilíř",
    "Room Středové náměstí is not in the bordering rooms of Hnízdo",
    "Room Na Ochozu is not in the bordering rooms of Přízemí",
    "Room Zahrádkářská Kolonie is not in the bordering rooms of Holy Hole to Hell",
    "Room Holy Hole to Hell is not in the bordering rooms of Bobův Dům",
    "Room Balkón svaté Neděle is not in the bordering rooms of Nether",
    "Room Tranzit is not in the bordering rooms of Ústřední středisko Beta",
    "Room Městský Okruh is not in the bordering rooms of Dálnice",
    "Room Můstek is not in the bordering rooms of Temná Strana Síly",
    "Room Guacamole is not in the bordering rooms of Výtahová Šachta",
    "Room Hlavní Koridor I is not in the bordering rooms of Přízemí",
    "Room Hlavní Koridor I is not in the bordering rooms of Na Ochozu",
    "Room Hlavní Koridor II is not in the bordering rooms of Středové náměstí",
    "Room Hlavní Koridor II is not in the bordering rooms of Hvězda",
    "Room Hlavní Koridor II is not in the bordering rooms of Kazeta",
    "Room Hlavní Koridor II is not in the bordering rooms of Dálniční Pilíř",
    "Room Hlavní Koridor II is not in the bordering rooms of Hnízdo",
    "Room Pod Schody is not in the bordering rooms of Na Ochozu",
    "Room Pod Schody is not in the bordering rooms of Hlavní Koridor I",
    "Room Pod Schody is not in the bordering rooms of Přízemí",
    "Room Zmatek is not in the bordering rooms of Venkovní Schodiště",
    "Room Rozhraní is not in the bordering rooms of Kazeta",
    "Room Rozhraní is not in the bordering rooms of The Airship",
    "Room Rozhraní is not in the bordering rooms of Hnízdo",
    "Room Špunt is not in the bordering rooms of Výtahový Můstek",
    "Room Železná Lhota is not in the bordering rooms of Dálniční Pilíř",
    "Room Výstavní is not in the bordering rooms of Le Pont",
    "Room Hnízdo is not in the bordering rooms of Dálniční Pilíř",
    "Room Hnízdo is not in the bordering rooms of Kazeta",
    "Room Loosova Vila is not in the bordering rooms of Jezírko",
    "Room Skokanský Stav is not in the bordering rooms of Jezírko",
    "Room Dálnice is not in the bordering rooms of Dálniční Pilíř",
    "Room Na Můstku is not in the bordering rooms of Vstup",
    "Room Svatyně is not in the bordering rooms of Garáž",
    "Room Východní Blok is not in the bordering rooms of Hráz",
    "Room Katedra is not in the bordering rooms of Veranda",
    "Room Katedra is not in the bordering rooms of Mrkev Tunel",
    "Room Katedra is not in the bordering rooms of Závodní Okruh",
    "Room Hráz is not in the bordering rooms of Dálniční Pilíř",
    "Room Jáma is not in the bordering rooms of Jezírko",
    "Room Labyrint is not in the bordering rooms of Výtahová Šachta",
    "Room Závodní Okruh is not in the bordering rooms of Veranda",
    "Room Závodní Okruh is not in the bordering rooms of Mrkev Tunel",
    "Room Zlaté Schodiště is not in the bordering rooms of V-Věž 2. podlaží",
    "Room Veranda is not in the bordering rooms of Výtahový Můstek",
    "Room Smyčka is not in the bordering rooms of Hráz",
    "Room Švihák is not in the bordering rooms of Garáž",
    "Room Za Vrbou is not in the bordering rooms of Pod Vrbou"
]

def check_bordering_rooms():
    for item_key in data.keys():
        item = data[item_key]
        if 'Type' in item.keys() and item['Type'] == 'místnost':
            for room in item['Bordering_rooms']:
                if item_key not in data[room]['Bordering_rooms']:
                    a = f"Room {item['Title']} is not in the bordering rooms of {data[room]['Title']}"
                    if (a not in bram):
                        print(a)

def check_subdivisions():
    allsubs = {}
    for item_key in data.keys():
        item = data[item_key]
        if 'Type' in item.keys() and item['Type'] == 'místnost':
            metadata = item.get('Metadata', {})
            if metadata["Blok"] in allsubs.keys() and allsubs[metadata["Blok"]] != metadata["Okrsek"] + metadata["Čtvrť"] + metadata["Část"]:
                print(f"Room {item['Title']} has a different subdivision for Blok {metadata['Blok']}: {allsubs[metadata['Blok']]} vs {metadata['Okrsek'] + metadata['Čtvrť'] + metadata['Část']}")
            allsubs[metadata["Blok"]] = metadata["Okrsek"] + metadata["Čtvrť"] + metadata["Část"]
    return allsubs

def get_all_subdivisions():
    blocks = {}
    okrseky = {}
    ctvrte = {}
    casti = {}
    for item_key in data.keys():
        item = data[item_key]
        if 'Type' in item.keys() and item['Type'] == 'místnost':
            metadata = item.get('Metadata', {})
            blok = metadata.get("Blok", "")
            okrsek = metadata.get("Okrsek", "")
            ctvrt = metadata.get("Čtvrť", "")
            cast = metadata.get("Část", "")
            if blok:
                blocks[blok] = [okrsek, ctvrt, cast]
            if okrsek:
                okrseky[okrsek] = [ctvrt, cast]
            if ctvrt:
                ctvrte[ctvrt] = cast
            if cast:
                casti[cast] = None
    return {
        "blocks": blocks,
        "okrseky": okrseky,
        "ctvrte": ctvrte,
        "casti": casti
    }

def subdivcheck():
    all_subdivisions = get_all_subdivisions()

    for blok_key in all_subdivisions['blocks'].keys():
        match = re.match(r"\[(.*?)\]\((.*?)\)", blok_key)
        if match:
            name, key = match.groups()
        else:
            name, key = blok_key, ""
        okrsek, ctvrt, cast = all_subdivisions['blocks'][blok_key]

        if key not in data.keys(): 
            print(f""""{key}": {{
            "Title": "{name}",
            "Type": "blok",
            "Images": [],
            "Image_titles": [],
            "Bordering_rooms": [],
            "Alternate_names": [],
            "Metadata": {{
                "Název": "{name}",
                "Okrsek": "{okrsek}",
                "Čtvrť": "{ctvrt}",
                "Část": "{cast}"
            }},
            "Sections": [
                {{
                    "Title": "Přehled",
                    "Content": ""
                }},
                {{
                    "Title": "Název",
                    "Content": ""
                }},
                {{
                    "Title": "Interiér",
                    "Content": ""
                }},
                {{
                    "Title": "Zajímavosti",
                    "Content": ""
                }},
                {{
                    "Title": "Aktuality",
                    "Content": ""
                }}
            ],
            "image_id": 0,
            "area": 0,
            "Empty": true,
            "numberOfRooms": 0,
            "redirect": null,
            "tobeedited": false
            }},""")

    for okrsek_key in all_subdivisions['okrseky'].keys():
        match = re.match(r"\[(.*?)\]\((.*?)\)", okrsek_key)
        if match:
            name, key = match.groups()
        else:
            name, key = okrsek_key, ""
        ctvrt, cast = all_subdivisions['okrseky'][okrsek_key]

        if key not in data.keys(): 
            print(f""""{key}": {{
            "Title": "{name}",
            "Type": "okrsek",
            "Images": [],
            "Image_titles": [],
            "Bordering_rooms": [],
            "Alternate_names": [],
            "Metadata": {{
                "Název": "{name}",
                "Čtvrť": "{ctvrt}",
                "Část": "{cast}"
            }},
            "Sections": [
                {{
                    "Title": "Přehled",
                    "Content": ""
                }},
                {{
                    "Title": "Název",
                    "Content": ""
                }},
                {{
                    "Title": "Interiér",
                    "Content": ""
                }},
                {{
                    "Title": "Zajímavosti",
                    "Content": ""
                }},
                {{
                    "Title": "Aktuality",
                    "Content": ""
                }}
            ],
            "image_id": 0,
            "area": 0,
            "Empty": true,
            "numberOfRooms": 0,
            "redirect": null,
            "tobeedited": false
            }},""")

    for ctvrt_key in all_subdivisions['ctvrte'].keys():
        match = re.match(r"\[(.*?)\]\((.*?)\)", ctvrt_key)
        if match:
            name, key = match.groups()
        else:
            name, key = okrsek_key, ""
        cast = all_subdivisions['ctvrte'][ctvrt_key]

        if key not in data.keys(): 
            print(f""""{key}": {{
            "Title": "{name}",
            "Type": "čtvrť",
            "Images": [],
            "Image_titles": [],
            "Bordering_rooms": [],
            "Alternate_names": [],
            "Metadata": {{
                "Název": "{name}",
                "Část": "{cast}"
            }},
            "Sections": [
                {{
                    "Title": "Přehled",
                    "Content": ""
                }},
                {{
                    "Title": "Název",
                    "Content": ""
                }},
                {{
                    "Title": "Interiér",
                    "Content": ""
                }},
                {{
                    "Title": "Zajímavosti",
                    "Content": ""
                }},
                {{
                    "Title": "Aktuality",
                    "Content": ""
                }}
            ],
            "image_id": 0,
            "area": 0,
            "Empty": true,
            "numberOfRooms": 0,
            "redirect": null,
            "tobeedited": false
            }},""")

    for cast_key in all_subdivisions['casti'].keys():
        match = re.match(r"\[(.*?)\]\((.*?)\)", cast_key)
        if match:
            name, key = match.groups()
        else:
            name, key = okrsek_key, ""
        if key not in data.keys(): 
            print(f""""{key}": {{
            "Title": "{name}",
            "Type": "čtvrť",
            "Images": [],
            "Image_titles": [],
            "Bordering_rooms": [],
            "Alternate_names": [],
            "Metadata": {{
                "Název": "{name}"
            }},
            "Sections": [
                {{
                    "Title": "Přehled",
                    "Content": ""
                }},
                {{
                    "Title": "Název",
                    "Content": ""
                }},
                {{
                    "Title": "Interiér",
                    "Content": ""
                }},
                {{
                    "Title": "Zajímavosti",
                    "Content": ""
                }},
                {{
                    "Title": "Aktuality",
                    "Content": ""
                }}
            ],
            "image_id": 0,
            "area": 0,
            "Empty": true,
            "numberOfRooms": 0,
            "redirect": null,
            "tobeedited": false
            }},""")

check_bordering_rooms()
check_subdivisions()
subdivcheck()