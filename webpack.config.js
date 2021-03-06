﻿const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const postcssPresetEnv = require('postcss-preset-env');
const webpack = require('webpack');

const bootstrapPackages = {
    Alert: 'exports-loader?Alert!bootstrap/js/src/alert',
    // Button: 'exports-loader?Button!bootstrap/js/src/button',
    Carousel: 'exports-loader?Carousel!bootstrap/js/src/carousel',
    Collapse: 'exports-loader?Collapse!bootstrap/js/src/collapse',
    // Dropdown: 'exports-loader?Dropdown!bootstrap/js/src/dropdown',
    Modal: 'exports-loader?Modal!bootstrap/js/src/modal',
    // Popover: 'exports-loader?Popover!bootstrap/js/src/popover',
    Scrollspy: 'exports-loader?Scrollspy!bootstrap/js/src/scrollspy',
    Tab: 'exports-loader?Tab!bootstrap/js/src/tab',
    // Tooltip: 'exports-loader?Tooltip!bootstrap/js/src/tooltip',
    Util: 'exports-loader?Util!bootstrap/js/src/util'
};

module.exports = (env, argv) => {
    let devMode = argv.mode !== 'production';

    return {
        mode: devMode ? 'development' : 'production',
        entry: {
            site: './wwwroot/js/site.js', 
            chat: './wwwroot/js/chat.js', 
            style: './wwwroot/sass/site.scss'
        },
        output: {
            path: path.resolve(__dirname, 'wwwroot/dist'),
            publicPath: '/css',
            filename: 'js/[name].js'
        },
        module: {
            rules: [
                {
                    test: /bootstrap(.)*\.js$/,
                    use: {
                        loader: 'babel-loader',
                        options: {
                            presets: ['@babel/preset-env'],
                            plugins: ['@babel/plugin-proposal-object-rest-spread'],
                            cacheDirectory: true
                        }
                    }
                },
                {
                    test: /\.(sa|sc)ss$/,
                    use: [
                        {
                            loader: MiniCssExtractPlugin.loader
                        },
                        {
                            loader: 'css-loader',
                            options: {
                                importLoaders: 2
                            }
                        },
                        {
                            loader: 'postcss-loader',
                            options: {
                                ident: 'postcss',
                                plugins: devMode
                                    ? () => []
                                    : () => [
                                        postcssPresetEnv({
                                            browsers: ['>1%']
                                        }),
                                        require('cssnano')()
                                    ]
                            }
                        },
                        {
                            loader: 'sass-loader'
                        }
                    ]
                },
                {
                    test: /\.(png|jpe?g|gif)$/,
                    use: [
                        {
                            loader: 'file-loader',
                            options: {
                                name: '[name].[ext]',
                                publicPath: '../../images',
                                emitFile: false
                            }
                        }
                    ]
                }
            ]
        },
        plugins: [
            new webpack.ProvidePlugin(bootstrapPackages),
            new MiniCssExtractPlugin({
                filename: 'css/site.css'
            })
        ]
    }
};
