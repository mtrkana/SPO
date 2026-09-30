function validateNumber(val) {
    if (typeof val === "number" && !isNaN(val)) {
        return val > 0 ? val : Math.abs(val);
    }
    return 0;
}
function processNumbersList(numbers) {
    let sum = 0, index = 0;
    while (index < numbers.length) {
        let current = validateNumber(numbers[index]);
        if (current % 2 === 0) {
            sum += current;
        } else if (current % 3 === 0) {
            sum += current * 2;
        } else {
            sum += 1;
        }
        index++;
    }
    let counter = 2;
    do {
        if (sum > 50) sum -= counter;
        else sum += counter;
        counter--;
    } while (counter > 0);
    return sum;
}
function handleCommand(command, factor) {
    let score = 0;
    if (factor > 0) {
        switch (command) {
            case "START":
                score = 10 * factor;
                break;
            case "PAUSE":
                score = 5 * factor;
                break;
            case "STOP":
                score = 0;
                break;
            default:
                score = -1;
                break;
        }
    }
    return score;
}
function analyzeRecords(recordsList) {
    let totalScore = 0, keys = [];
    for (let record of recordsList) {
        if (typeof record === "object" && record !== null) {
            for (let key in record) {
                let val = record[key];
                if (Array.isArray(val)) {
                    totalScore += processNumbersList(val);
                } else if (typeof val === "string") {
                    totalScore += handleCommand(val, 2);
                } else {
                    totalScore += validateNumber(val);
                }
                keys.push(key);
            }
        } else if (typeof record === "number") {
            let isPositive = record >= 0 ? true : false;
            totalScore += isPositive ? record : 0;
        }
    }
    for (let i = 0; i < keys.length; i++) {
        if (keys[i].length > 4) totalScore += 2;
    }
    return totalScore;
}
function main() {
    console.log("начало работы");
    let data = [{ nums: [10, 15], mode: "START" }, 50];
    let res = analyzeRecords(data);
    console.log("Итоговый результат обработки:", res);
}
main();
