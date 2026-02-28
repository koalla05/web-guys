import re
import json
from pathlib import Path

import pdfplumber
import pandas as pd

PDF_PATH = Path("pub718.pdf")
OUT_CSV = Path("tax_rates.csv")

STATE = "NY"
STATE_RATE = 0.04
MCTD_RATE = 0.00375

FRACTIONS = {"¼":0.25,"½":0.5,"¾":0.75,"⅛":0.125,"⅜":0.375,"⅝":0.625,"⅞":0.875}


def pct(s):
    s = (s or "").strip().replace(" ", "")
    m = re.match(r"^(\d+)([¼½¾⅛⅜⅝⅞])?$", s)
    if not m:
        raise ValueError(f"Bad rate: {s!r}")
    return (float(m.group(1)) + FRACTIONS.get(m.group(2), 0.0)) / 100.0


def strip_star(name):
    return name.startswith("*"), name.lstrip("*").strip()


def is_see_nyc(name):
    return bool(re.search(r"\bsee New York City\b", name, re.IGNORECASE))


def county_name(raw):
    return re.sub(r"\s*[–\-]\s*except\s*$", "", raw, flags=re.IGNORECASE).strip()


def city_name(raw):
    return re.sub(r"\s*\(city\)\s*$", "", raw, flags=re.IGNORECASE).strip()


def build_row(county, city, code, composite, has_star):
    sp = [{"name": "MCTD", "rate": MCTD_RATE}] if has_star else []
    sp_total = MCTD_RATE if has_star else 0.0
    if county.lower() == "new york state only":
        local, sp = 0.0, []
    else:
        local = composite - STATE_RATE - sp_total
    return {
        "state": STATE,
        "county": county,
        "city": city,
        "reporting_code": str(code).strip().zfill(4) if code else "",
        "special_districts": json.dumps([s["name"] for s in sp]),
        "composite_tax_rate": round(composite, 10),
        "state_rate": STATE_RATE,
        "county_rate": round(0.0 if city else local, 10),
        "city_rate": round(local if city else 0.0, 10),
        "special_rates": json.dumps(sp),
    }


def parse_pub718_table(pdf_path: Path) -> pd.DataFrame:
    with pdfplumber.open(str(pdf_path)) as pdf:
        table = pdf.pages[0].extract_table({
            "vertical_strategy": "lines", "horizontal_strategy": "lines",
            "snap_tolerance": 3, "join_tolerance": 3, "intersection_tolerance": 3,
        })
    if not table:
        raise RuntimeError("No table extracted.")

    nyc_rate = nyc_code = None
    for row in table[1:]:
        cells = list(row) + [None] * 9
        for c in range(0, 9, 3):
            loc = cells[c]
            if loc and loc.lstrip("*").strip().lower() == "new york city":
                nyc_rate = pct(cells[c+1])
                nyc_code = (cells[c+2] or "").strip()
                break
        if nyc_rate:
            break
    if nyc_rate is None:
        raise RuntimeError("Could not find NYC row.")
    
    entries = []
    pending = {0: [], 3: [], 6: []}
    last_entry = {0: None, 3: None, 6: None}

    for row in table[1:]:
        cells = list(row) + [None] * 9
        for c in range(0, 9, 3):
            loc_raw = cells[c]
            rate_str = (cells[c+1] or "").strip()
            code_str = (cells[c+2] or "").strip()

            if loc_raw is not None:
                lines = [l.strip() for l in loc_raw.split("\n") if l.strip()]
                if not lines:
                    continue

                header_line = lines[0]
                city_lines = lines[1:]

                has_star, header_clean = strip_star(header_line)

                if is_see_nyc(header_clean):
                    borough = re.sub(r"\s*[–\-]\s*see\s+New York City\s*$", "",
                                     header_clean, flags=re.IGNORECASE).strip()
                    entry = {"county": borough, "has_star": has_star,
                             "composite": nyc_rate, "code": nyc_code, "cities": []}
                    entries.append(entry)
                    last_entry[c] = entry
                    pending[c] = []

                elif header_clean.lower() == "new york city":
                    entry = {"county": "New York City", "has_star": has_star,
                             "composite": pct(rate_str) if rate_str else nyc_rate,
                             "code": code_str or nyc_code, "cities": []}
                    entries.append(entry)
                    last_entry[c] = entry
                    pending[c] = []

                else:
                    composite = pct(rate_str) if rate_str else None
                    entry = {"county": header_clean, "has_star": has_star,
                             "composite": composite, "code": code_str, "cities": []}
                    entries.append(entry)
                    last_entry[c] = entry
                    pending[c] = list(city_lines)

            else:
                if (rate_str or code_str) and pending[c] and last_entry[c]:
                    city_raw = pending[c].pop(0)
                    cstar, craw = strip_star(city_raw)
                    cname = city_name(craw)
                    c_composite = pct(rate_str) if rate_str else last_entry[c]["composite"]
                    last_entry[c]["cities"].append({
                        "name": cname,
                        "composite": c_composite,
                        "code": code_str,
                        "has_star": cstar or last_entry[c]["has_star"],
                    })

    out = []
    for e in entries:
        raw = e["county"]
        cnty = county_name(raw)
        composite = e["composite"]
        if composite is None:
            continue

        if raw.lower() == "new york city":
            out.append(build_row("New York City", "New York City", e["code"], composite, e["has_star"]))
        else:
            out.append(build_row(cnty, "", e["code"], composite, e["has_star"]))

        for city in e["cities"]:
            out.append(build_row(cnty, city["name"], city["code"],
                                  city["composite"], city["has_star"]))

    df = pd.DataFrame(out).drop_duplicates(subset=["state", "county", "city", "reporting_code"])
    df["reporting_code"] = df["reporting_code"].astype(str)
    df["city"] = df["city"].fillna("0").replace("", "0")
    df.to_csv(OUT_CSV, index=False, quoting=1)
    print(f"Saved {len(df)} rows -> {OUT_CSV.resolve()}")
    return df


if __name__ == "__main__":
    df = parse_pub718_table(PDF_PATH)
    pd.set_option("display.max_rows", 150)
    pd.set_option("display.max_colwidth", 35)
