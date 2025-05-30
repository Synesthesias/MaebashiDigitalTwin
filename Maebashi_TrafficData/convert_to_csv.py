import os
import re
import csv
from glob import glob
import shutil

# 既存CSVからレーン名→リンクIDのマッピングを作成
lane_to_linkid = {}
with open('Maebashi_TrafficData/lane_to_linkid.csv', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    for row in reader:
        lane_to_linkid[row['lanename']] = row['linkid']

input_files = sorted(glob('Maebashi_TrafficData/1月*日_*.txt'))
print('input_files:', input_files, flush=True)

try:
    # csvs_pyフォルダがあれば削除して再作成
    csv_dir = 'Maebashi_TrafficData/csvs_py'
    if os.path.exists(csv_dir):
        shutil.rmtree(csv_dir)
    os.makedirs(csv_dir, exist_ok=True)

    header = ['集計開始時刻', '集計終了時刻', 'リンクID', '交通量', 'レーン名']

    for file in input_files:
        print('Processing:', file, flush=True)
        with open(file, encoding='utf-8') as f:
            text = f.read()
        # ブロックごとに分割（A/B/C...）
        blocks = re.split(r'自\s*動\s*車\s*交通量\s*調査結果集計表', text)
        block_label = ord('A')
        for block in blocks:
            tables = re.split(r'時\s*間\s*帯', block)
            # tables[1]: in, tables[2]: out, tables[3]: 合計（無視）
            for i in range(1, len(tables)-1, 3):
                csv_rows = []
                for ttype, table in zip(['in', 'out'], [tables[i], tables[i+1] if i+1 < len(tables) else None]):
                    if table is None:
                        continue
                    # 全車合計行を抽出
                    m = re.search(r'全\s*車\s*合\s*計\s*([0-9\s]+)', table)
                    if not m:
                        continue
                    values = re.findall(r'\d+', m.group(1))
                    if len(values) < 13:
                        continue
                    for j in range(13):
                        # start_hour, end_hourをYYYYMMDDHHMM形式に
                        start_hour = f'{7+j:02d}:00'
                        end_hour = f'{8+j:02d}:00'
                        # ファイル名から番号を抽出
                        file_num = re.search(r'_(\d+)\.txt$', file)
                        file_num = file_num.group(1) if file_num else 'X'
                        block_name = chr(block_label)
                        lane = f'Link{file_num}_{block_name}_{ttype}'
                        # レーン名からリンクIDを取得
                        linkid = lane_to_linkid.get(lane, 'Unknown')
                        date = '20240116' if '1月16日' in file else '20240109'
                        # 時間を結合してYYYYMMDDHHMM形式に
                        start_time = date + start_hour.replace(':', '')
                        end_time = date + end_hour.replace(':', '')
                        csv_rows.append([
                            start_time,
                            end_time,
                            linkid,
                            values[j],
                            lane
                        ])
                if csv_rows:
                    outname = os.path.basename(file).replace('.txt', f'_{chr(block_label)}.csv')
                    outpath = os.path.join(csv_dir, outname)
                    print('Writing:', outpath, flush=True)
                    with open(outpath, 'w', encoding='utf-8', newline='') as out:
                        writer = csv.writer(out)
                        writer.writerow(header)
                        writer.writerows(csv_rows)

                # 1月9日ならBをスキップ
                if '1月9日' in file and '_1' in file and block_label == ord('A'):
                    block_label += 2  # A→C
                else:
                    block_label += 1
except Exception as e:
    print('Error:', e, flush=True) 