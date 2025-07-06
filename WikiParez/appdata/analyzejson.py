import json
import re

json_data = open('pagesData.json', 'r', encoding='utf-8').read()
data = json.loads(json_data)

def check_bordering_rooms():
    for item_key in data.keys():
        item = data[item_key]
        if 'Type' in item.keys() and item['Type'] == 'místnost':
            for room in item['Bordering_rooms']:
                if item_key not in data[room]['Bordering_rooms']:
                    print(f"Room {item['Title']} is not in the bordering rooms of {data[room]['Title']}")

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
        "Empty": false,
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
        "Empty": false,
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
        "Empty": false,
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
        "Empty": false,
        "numberOfRooms": 0,
        "redirect": null,
        "tobeedited": false
        }},""")
