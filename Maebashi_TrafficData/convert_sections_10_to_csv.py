import os
import re
import csv
import shutil

try:
    print('Start convert_sections_10_to_csv.py', flush=True)
    # 出力先フォルダを初期化
    csv_dir = 'Maebashi_TrafficData/csvs_py_sections_10'
    if os.path.exists(csv_dir):
        shutil.rmtree(csv_dir)
    os.makedirs(csv_dir, exist_ok=True)

    # 既存CSVからレーン名→リンクIDのマッピングを作成
    lane_to_linkid = {}
    with open('Maebashi_TrafficData/lane_to_linkid.csv', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            lane_to_linkid[row['lanename']] = row['linkid']

    # 解析対象ファイル（10用）
    input_files = [
        'Maebashi_TrafficData/1月9日_10_A_in.txt',
        'Maebashi_TrafficData/1月9日_10_A_out.txt',
        'Maebashi_TrafficData/1月9日_10_B_in.txt',
        'Maebashi_TrafficData/1月9日_10_B_out.txt',
        'Maebashi_TrafficData/1月9日_10_C_in.txt',
        'Maebashi_TrafficData/1月9日_10_C_out.txt',
        'Maebashi_TrafficData/1月9日_10_D_in.txt',
        'Maebashi_TrafficData/1月9日_10_D_out.txt',
        'Maebashi_TrafficData/1月16日_10_A_in.txt',
        'Maebashi_TrafficData/1月16日_10_A_out.txt',
        'Maebashi_TrafficData/1月16日_10_B_in.txt',
        'Maebashi_TrafficData/1月16日_10_B_out.txt',
        'Maebashi_TrafficData/1月16日_10_C_in.txt',
        'Maebashi_TrafficData/1月16日_10_C_out.txt',
        'Maebashi_TrafficData/1月16日_10_D_in.txt',
        'Maebashi_TrafficData/1月16日_10_D_out.txt',
    ]

    header = ['集計開始時刻', '集計終了時刻', 'リンクID', '交通量', 'レーン名']

    for file in input_files:
        print(f'Processing: {file}', flush=True)
        with open(file, encoding='utf-8') as f:
            lines = [line.strip() for line in f if line.strip()]
        # データ行の開始位置を自動検出（「7時台」で始まる行を探す）
        data_start = None
        for idx, line in enumerate(lines):
            if re.match(r'7時台', line):
                data_start = idx
                break
        if data_start is None:
            print('データ開始行が見つかりません', flush=True)
            continue
        data_lines = lines[data_start:data_start+13]
        values = []
        for dl in data_lines:
            dl = dl.replace('\t', ' ')
            dl = re.sub(r' +', ' ', dl)
            cols = dl.split(' ')
            # 合計値はindex=7
            goukei = cols[7] if len(cols) > 7 else ''
            values.append(goukei.replace(',', ''))
        # ファイル名から日付・番号・断面名・in/outを抽出
        m = re.match(r'.*([0-9]{1,2})月([0-9]{1,2})日_([0-9]+)_([A-Z])_(in|out)\.txt', os.path.basename(file))
        if not m:
            print('ファイル名から情報が抽出できません', flush=True)
            continue
        month, day, num, section, inout = m.groups()
        date = f'2024{int(month):02d}{int(day):02d}'
        lane = f'Link{num}_{section}_{inout}'
        linkid = lane_to_linkid.get(lane, 'Unknown')
        csv_rows = []
        for j in range(13):
            start_hour = f'{7+j:02d}00'
            end_hour = f'{8+j:02d}00'
            start_time = date + start_hour
            end_time = date + end_hour
            csv_rows.append([
                start_time,
                end_time,
                linkid,
                values[j],
                lane
            ])
        outname = os.path.basename(file).replace('.txt', '.csv')
        outpath = os.path.join(csv_dir, outname)
        print(f'Writing: {outpath} ({len(csv_rows)} rows)', flush=True)
        with open(outpath, 'w', encoding='utf-8', newline='') as out:
            writer = csv.writer(out)
            writer.writerow(header)
            writer.writerows(csv_rows)
except Exception as e:
    print('Error:', e, flush=True) 