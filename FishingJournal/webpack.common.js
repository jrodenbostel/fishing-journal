var path = require('path');
const webpack = require('webpack');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

module.exports = {
    entry: {
        layout: './Scripts/app.js'
    },
    output: {
        path: path.resolve(__dirname, 'wwwroot'),
        filename: 'js/[name].js',
        publicPath: '/'
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: 'css/site.css',
            chunkFilename: '[name].css'
        })
    ],
    module: {
        rules: [
            {
                test: /\.(png|jpg|jpeg|gif|woff|woff2|ttf|eot|svg)(\?.*)?$/,
                loader: 'file-loader?name=fonts/[name].[ext]',
            },
            {
                test: /\.s[ac]ss$/i,
                use: [MiniCssExtractPlugin.loader, 'css-loader', 'sass-loader'],
            },
        ]
    }
};