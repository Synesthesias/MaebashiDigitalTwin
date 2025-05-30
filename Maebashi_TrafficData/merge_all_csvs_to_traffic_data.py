import os
import csv
from glob import glob

print('スクリプト開始', flush=True)

# 出力ファイル
out_20240109 = 'Assets/Runtime/Dashboard/Resources/traffic_data_20240109.csv'
out_20240116 = 'Assets/Runtime/Dashboard/Resources/traffic_data_20240116.csv'

# 入力フォルダ
input_dirs = [
    'Maebashi_TrafficData/csvs_py',
    'Maebashi_TrafficData/csvs_py_sections_7',
    'Maebashi_TrafficData/csvs_py_sections_10',
]

# ヘッダー
header = ['start', 'end', 'linkid', 'volume', 'lanename']

# データ格納用
rows_09 = []
rows_16 = []

def get_date_from_row(row):
    # start列から日付判定（2024年も2025年も許容）
    if row[0].startswith('20240109') or row[0].startswith('20250109'):
        return '20250109'
    elif row[0].startswith('20240116') or row[0].startswith('20250116'):
        return '20250116'
    return None

for d in input_dirs:
    print(f'ディレクトリ探索: {d}', flush=True)
    for file in glob(os.path.join(d, '*.csv')):
        print(f'  ファイル発見: {file}', flush=True)
        with open(file, encoding='utf-8') as f:
            reader = csv.reader(f)
            for row in reader:
                # 5列かつstart列が2024/2025年の日付で始まる行のみ抽出
                if len(row) == 5 and (row[0].startswith('20240109') or row[0].startswith('20240116') or row[0].startswith('20250109') or row[0].startswith('20250116')):
                    date = get_date_from_row(row)
                    if date == '20250109':
                        rows_09.append(row)
                    elif date == '20250116':
                        rows_16.append(row)
                else:
                    print(f'    スキップ: {row}', flush=True)

print(f'1月9日データ件数: {len(rows_09)}', flush=True)
print(f'1月16日データ件数: {len(rows_16)}', flush=True)

# --- linkid_mapをlane_to_linkid.csvの全レーンで作成 ---
linkid_map = {}
try:
    with open('Maebashi_TrafficData/lane_to_linkid.csv', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            linkid_map[row['lanename']] = row['linkid']
except Exception as e:
    print('linkid_map取得エラー:', e, flush=True)

# --- Link1_B_in/out・Link2_D_in/out完全自動生成（lane_to_linkid.csvのみ参照） ---
try:
    for lanename in ('Link1_B_in', 'Link1_B_out', 'Link2_D_in', 'Link2_D_out'):
        linkid = linkid_map.get(lanename, 'Unknown')
        for j in range(13):
            start_hour = f'{7+j:02d}00'
            end_hour = f'{8+j:02d}00'
            start = '20240109' + start_hour
            end = '20240109' + end_hour
            volume = '0'
            exists = any(r[0] == start and r[1] == end and r[4] == lanename for r in rows_09)
            if exists:
                continue
            rows_09.append([start, end, linkid, volume, lanename])
except Exception as e:
    print('Link1_B/2_D_in/out自動生成エラー:', e, flush=True)

# --- 並び順をlanenameでソート ---
rows_09.sort(key=lambda x: (x[4], x[0]))

# 日付ごとに出力
with open(out_20240109, 'w', encoding='utf-8', newline='') as f:
    writer = csv.writer(f)
    writer.writerow(header)
    writer.writerows(rows_09)
    print(f'書き出し完了: {out_20240109} ({len(rows_09)}行)', flush=True)

with open(out_20240116, 'w', encoding='utf-8', newline='') as f:
    writer = csv.writer(f)
    writer.writerow(header)
    writer.writerows(rows_16)
    print(f'書き出し完了: {out_20240116} ({len(rows_16)}行)', flush=True) 